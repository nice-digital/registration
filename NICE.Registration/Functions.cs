using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using NICE.Registration.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace NICE.Registration
{
	public class Functions
    {
        // This const is the name of the environment variable that the serverless.template will use to set
        // the name of the DynamoDB table used to store blog posts.
        const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "RegistrationTableName";

        IDynamoDBContext DDBContext { get; set; }

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            // Check to see if a table name was passed in through environment variables and if so 
            // add the table mapping.
            var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
            if(string.IsNullOrEmpty(tableName))
            {
                tableName = "Registration-alpha";
            }

            AWSConfigsDynamoDB.Context.TypeMappings[typeof(RegistrationSubmission)] = new Amazon.Util.TypeMapping(typeof(RegistrationSubmission), tableName);

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        /// <summary>
        /// Constructor used for testing passing in a preconfigured DynamoDB client.
        /// </summary>
        /// <param name="ddbClient"></param>
        /// <param name="tableName"></param>
        public Functions(IAmazonDynamoDB ddbClient, string tableName)
        {
            if (!string.IsNullOrEmpty(tableName))
            {
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Models.Registration)] = new Amazon.Util.TypeMapping(typeof(Models.Registration), tableName);
            }

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(ddbClient, config);
        }

        private (string nameIdentifier, APIGatewayProxyResponse response) GetUserNameFromAuthorisationHeader(APIGatewayProxyRequest request, ILambdaContext context)
        {
	        if (!AuthenticationHeaderValue.TryParse(request.Headers[Microsoft.Net.Http.Headers.HeaderNames.Authorization], out var authenticationHeaderValue))
	        {
		        return (nameIdentifier:null, response: new APIGatewayProxyResponse
		        {
			        StatusCode = (int)HttpStatusCode.Unauthorized,
			        Body = "Authorization header not found"
		        });
	        }
	        var jwtSecurityToken = new JwtSecurityToken(authenticationHeaderValue.Parameter);
	        var userNameIdentifier = jwtSecurityToken.Subject;

	        if (string.IsNullOrEmpty(userNameIdentifier))
	        {
		        context.Logger.LogLine("Username not found in JWT token. This is a big problem!");
		        return (nameIdentifier: null, response: new APIGatewayProxyResponse
                {
			        StatusCode = (int)HttpStatusCode.InternalServerError,
			        Body = "Username not found"
		        });
	        }

	        return (userNameIdentifier, null);
        }

        /// <summary>
        /// A Lambda function that returns back all the registrations for a user - paging through the dynamodb table if necessary
        /// </summary>
        /// <param name="request">the UserNameIdentifier identifier needs to be passed by querystring or path parameter</param>
        /// <returns>The list of registrations</returns>
        [Authorize(Policy = "Bearer")]
        public async Task<APIGatewayProxyResponse> GetRegistrationsForUserAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
            const string nameIdentifierPropertyName = nameof(RegistrationSubmission.UserNameIdentifier);

            var (userNameIdentifier, response) = GetUserNameFromAuthorisationHeader(request, context);
            if (response != null)
            {
	            return response;
            }

            context.Logger.LogLine($"Getting registrations for {userNameIdentifier}");

            var search = this.DDBContext.ScanAsync<RegistrationSubmission>(new List<ScanCondition>()
            {
	            new ScanCondition(nameIdentifierPropertyName, Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, userNameIdentifier)
            });
            var registrationsForUser = new List<RegistrationSubmission>();

            context.Logger.LogLine("About to query dynamodb");
            do
            {
	            var pageOfRegistrations = await search.GetNextSetAsync();
	            context.Logger.LogLine($"page result count: {pageOfRegistrations.Count}");
                registrationsForUser.AddRange(pageOfRegistrations);
            } while (!search.IsDone);

            context.Logger.LogLine($"Found {registrationsForUser.Count} registration submissions");

            var registrations = new Registrations(registrationsForUser);

            context.Logger.LogLine($"Total of {registrations.AllRegistrations.Count()} registrations");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(registrations.AllRegistrations, new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase}), 
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }

        /// <summary>
        /// A Lambda function that adds a registration.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Bearer")]
        public async Task<APIGatewayProxyResponse> AddRegistrationAsync(APIGatewayProxyRequest request, ILambdaContext context)
        {
	        var (userNameIdentifier, response) = GetUserNameFromAuthorisationHeader(request, context);
	        if (response != null)
	        {
		        return response;
	        }

            var registration = JsonSerializer.Deserialize<RegistrationSubmission>(request?.Body);

            if (!registration.Interests.Any())
            {
	            return new APIGatewayProxyResponse
	            {
		            StatusCode = (int) HttpStatusCode.InternalServerError,
		            Body = "No interests found"
	            };
            }

            registration.Id = Guid.NewGuid().ToString();
            registration.UserNameIdentifier = userNameIdentifier;

            context.Logger.LogLine($"Saving registration with id {registration.Id}");
            await DDBContext.SaveAsync<RegistrationSubmission>(registration);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = registration.Id.ToString(),
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }
    }
}
