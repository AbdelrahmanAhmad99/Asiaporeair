namespace Application.DTOs.ContextualPricingAttribute
{
    // DTO representing the context of a booking to find matching pricing rules
    public class PricingContextDto
    {
        public int DaysToDeparture { get; set; }
        public int LengthOfStayDays { get; set; }
        // ... other factors like 'UserLoyaltyLevel' could be added here
    }
}