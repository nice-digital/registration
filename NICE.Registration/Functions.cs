using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Serialization.SystemTextJson;
using NICE.Registration.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.CamelCaseLambdaJsonSerializer))]

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
                tableName = "Registration-local";
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
                AWSConfigsDynamoDB.Context.TypeMappings[typeof(Models.RegistrationSubmission)] = new Amazon.Util.TypeMapping(typeof(Models.RegistrationSubmission), tableName);
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

            context.Logger.LogLine($"nameIdentifier found: {userNameIdentifier}");

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

            var registrations = new RegistrationsTable(registrationsForUser);

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

	        string jsonToDeserialise = request?.Body.Trim();

            context.Logger.LogLine($"About to deserialise:<start>{jsonToDeserialise}<end>");
            context.Logger.LogLine($"About to deserialise with lambda system text serialiser:<start>{jsonToDeserialise}<end>");

            //var serialiser = new Amazon.Lambda.Serialization.Json.JsonSerializer();
			// var registration = serialiser.Deserialize<RegistrationSubmission>(jsonToDeserialise);

			var serialiser = new Amazon.Lambda.Serialization.SystemTextJson.CamelCaseLambdaJsonSerializer();

			var byteArray = Encoding.UTF8.GetBytes(jsonToDeserialise);
			var stream = new MemoryStream(byteArray);
			var registration = serialiser.Deserialize<RegistrationSubmission>(stream);

			//var registration = JsonSerializer.Deserialize<RegistrationSubmission>(jsonToDeserialise);
			context.Logger.LogLine("deserialised:");

            if (registration.Projects == null || !registration.Projects.Any())
            {
                context.Logger.LogLine("no projects found");
                context.Logger.LogLine($"original json: {jsonToDeserialise}");
                //JSONConvert
                
                //context.Logger.LogLine($"deserialised and reserialised json: {serialiser.Serialize<RegistrationSubmission>(registration)}");

                return new APIGatewayProxyResponse
	            {
		            StatusCode = (int) HttpStatusCode.InternalServerError,
		            Body = "{\"errormessage\": \"No projects found\"}"
                };
            }

            registration.Id = Guid.NewGuid().ToString();
            context.Logger.LogLine($"setting name identifier to: {userNameIdentifier}");
            registration.UserNameIdentifier = userNameIdentifier;
            context.Logger.LogLine("set name identifier");

            context.Logger.LogLine($"Saving registration with id {registration.Id}");
            await DDBContext.SaveAsync<RegistrationSubmission>(registration);

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = "{\"id\": \"" + registration.Id + "\"}",
                Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
            };
        }

        /// <summary>
        /// A Lambda function that returns back the users information from the last registration
        /// </summary>
        /// <param name="request">the UserNameIdentifier identifier needs to be passed by querystring or path parameter</param>
        /// <returns>The users information form the last registration</returns>
        [Authorize(Policy = "Bearer")]
        public async Task<APIGatewayProxyResponse> GetRegDataForFormPrePopulationAsync(APIGatewayProxyRequest request, ILambdaContext context)
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

            var pageOfRegistrations = await search.GetNextSetAsync();
            var registrationsForUser = pageOfRegistrations.FirstOrDefault();

            context.Logger.LogLine(registrationsForUser != null ? $"Found 1 submission {registrationsForUser.Id}" : $"Found 0 submissions");

            return new APIGatewayProxyResponse
            {
                StatusCode = (int)HttpStatusCode.OK,
                Body = JsonSerializer.Serialize(registrationsForUser, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
            };
        }
    }
}
