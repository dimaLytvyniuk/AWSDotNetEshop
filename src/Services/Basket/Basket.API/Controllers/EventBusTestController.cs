using EventBusSns;

namespace Basket.API.Controllers
{
    [Route("api/eventbus/test")]
    public class EventBusTestController : ControllerBase
    {
        private readonly IAmazonQueueEventBus _eventBus;

        public EventBusTestController(IAmazonQueueEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAsync()
        {
            var checkoutEvent = new UserCheckoutAcceptedIntegrationEvent(
                userId: Guid.NewGuid().ToString(),
                userName: "testName",
                city: "NightCity",
                street: "TestStreet",
                state: "Az",
                country: "UA",
                zipCode: "02224",
                cardNumber: "1234",
                cardHolderName: "The Best Buyer",
                cardExpiration: DateTime.UtcNow,
                cardSecurityNumber: "234",
                cardTypeId: 1,
                buyer: "The Best",
                requestId: Guid.NewGuid(),
                basket: new CustomerBasket()
            );

            await _eventBus.Publish(checkoutEvent, "eshop");

            return Ok("Test connection");
        }
    }
}
