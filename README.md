# RateLimiting
Helper library for rate limitng 


To limit the amount of requests to a controller action, one needs to implement rate limiting. 
Here one makes a choice of the time duration to quantify as a session and the number of requests that 
can be made during that session. 

In this implementation, a memory cache is used to keep count of how many requests have been made so far and only incremented 
in the case of a an ok result.


The attribute can then be used in a controller action to track the number of requests made to that endpoint
for example 

```C#
   public class RecordsController : ControllerBase
      {
        private readonly MainDbContext dbContext;
        public ReferenceController(MainDbContext dbContext)
        {            
            this.dbContext=dbContext?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// Gets a crb record given the  id
        /// </summary>
        /// <param name="id">unique identifier for a record</param>
        /// <returns></returns>
        [HttpGet]
        [RateLimiting]
        [ProducesResponseType(typeof(Record), 200)]
        [ProducesResponseType(typeof(ErrorModel), 400)]
        public async Task<IActionResult> GetRecordAsync([FromQuery] string id)
        {
            // ensure id is provided
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ErrorModel
                {
                    Code = ConstStrings.ErrorCodes.MissingField,
                    Description = $"You must specify '{nameof(id)}'"
                });
            }
            
            var record= awa dbContext.Records.FirstOrDefaultAsync(x=>x.Id=id);
            
            return Ok(record);
          }
        }
          
    ```
