using DataApi.Dtos;
namespace DataApi.Service;

public interface IGetDataService
{
    Task<IEnumerable<FullStationDto>> FilterStation(int? minAvailableBikes, int? isRenting, int? isRerurning);
}
