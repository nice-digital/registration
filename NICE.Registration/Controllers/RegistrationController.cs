using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NICE.Registration.Controllers
{
	[Route("api/[controller]")]
    public class RegistrationController : ControllerBase
    {
	    // This const is the name of the environment variable that the serverless.template will use to set
	    // the name of the DynamoDB table used to store blog posts.
	    const string TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP = "RegistrationTableName";

	    IDynamoDBContext DDBContext { get; set; }

	    public RegistrationController()
	    {
		    // Check to see if a table name was passed in through environment variables and if so 
		    // add the table mapping.
		    var tableName = System.Environment.GetEnvironmentVariable(TABLENAME_ENVIRONMENT_VARIABLE_LOOKUP);
		    if (string.IsNullOrEmpty(tableName))
		    {
			    tableName = "Registration-local";
		    }

		    AWSConfigsDynamoDB.Context.TypeMappings[typeof(Models.Registration)] = new Amazon.Util.TypeMapping(typeof(Models.Registration), tableName);

		    var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
		    this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }


	    // GET api/Registration?nameIdentifier=x
        [HttpGet]
        public async Task<object> Get(string nameIdentifier)
        {
            const string nameIdentifierPropertyName = nameof(Models.Registration.UserNameIdentifier);
            
            if (string.IsNullOrEmpty(nameIdentifier))
            {
	            return BadRequest($"Invalid parameter: {nameof(nameIdentifier)}");
            }

            //context.Logger.LogLine($"Getting registrations for {nameIdentifier}");

            var search = this.DDBContext.ScanAsync<Models.Registration>(new List<ScanCondition>()
            {
                new ScanCondition(nameIdentifierPropertyName, Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, nameIdentifier)
            });
            var registrationsForUser = new List<Models.Registration>();

            //context.Logger.LogLine("About to query dynamodb");
            do
            {
                var pageOfRegistrations = await search.GetNextSetAsync();
                //context.Logger.LogLine($"page result count: {pageOfRegistrations.Count}");
                registrationsForUser.AddRange(pageOfRegistrations);
            } while (!search.IsDone);

            //context.Logger.LogLine($"Found {registrationsForUser.Count} registrations");

            return Ok(registrationsForUser);
        }

        //POST api/Registration - with a json body
        [HttpPost]
        public async Task Post([FromBody]Models.Registration registration)
        {
	        //var registration = JsonSerializer.Deserialize<Models.Registration>(request?.Body);

	        registration.Id = Guid.NewGuid().ToString();

	        //context.Logger.LogLine($"Saving registration with id {registration.Id}");
	        await DDBContext.SaveAsync<Models.Registration>(registration);
        }
    }
}
