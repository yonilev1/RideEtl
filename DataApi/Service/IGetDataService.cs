using DataApi.Dto_s;
using DataApi.Dtos;
using DataApi.Models;
namespace DataApi.Service;

public interface IGetDataService
{
    Task<IEnumerable<FullStationDto>> FilterStation(int? minAvailableBikes, int? isRenting, int? isRerurning);
    Task<FullStationDto?> GetStationById(string stationId);
    Task<FullStationsStatusDto?> GetStationStatusById(string stationId);

}
