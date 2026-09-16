namespace DataApi.Dto_s;

public class GetByTimeDto
{
    public long TimeStamp { get; set; }
    public int NumBikesAvailable { get; set; }
    public int NumDocksAvailable { get; set; }
}
