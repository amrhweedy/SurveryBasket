namespace FileManager.Entities;

public sealed class UploadedFile
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string FileName { get; set; } = string.Empty;   // original file name
    public string StoredFileName { get; set; } = string.Empty; // fake file name
    public string ContentType { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;

}

//public Guid Id { get; set; } = Guid.CreateVersion7(); => start from .net 9

//Every time an instance of UploadedFile is created, a new UUID version 7 is generated for the Id property.

//This UUID is time-ordered, meaning that if you create multiple UploadedFile instances, their Id values will be sortable by the time they were created.

//Benefits of Using UUID Version 7
//Sortable by Time: Since the UUID includes a timestamp, you can sort records by their Id to determine the order in which they were created.

//Database Efficiency: Time-ordered UUIDs are more efficient for indexing and querying in databases compared to random UUIDs.

//Uniqueness: The random bits ensure that the UUIDs remain globally unique.

// example => 018f0a7d-7a3b-7c00-9e1a-0242ac130003

//The first part(018f0a7d-7a3b-7c00) contains the timestamp.
//The remaining part(9e1a-0242ac130003) contains random bits for uniqueness.