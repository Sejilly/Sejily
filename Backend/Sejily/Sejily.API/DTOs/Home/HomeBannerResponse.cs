namespace Sejily.API.DTOs.Home;

// The home banner: how many access requests wait for an answer, and the one that
// expires first. Item is null when Count is 0.
public class HomeBannerResponse
{
    public int Count { get; set; }

    // The app draws its countdowns from this, never from the phone clock.
    public DateTime ServerTime { get; set; }

    public HomeBannerItem? Item { get; set; }
}
