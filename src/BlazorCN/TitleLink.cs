namespace BlazorCN;

internal sealed class TitleLink
{
    public string Id { get; }
    public bool IsLinked { get; set; }

    public TitleLink(string id) => Id = id;
}
