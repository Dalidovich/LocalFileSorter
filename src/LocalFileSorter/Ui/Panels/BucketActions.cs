using LocalFileSorter.Common.Model;

namespace LocalFileSorter.Ui.Panels;

public sealed record BucketActions(Action Sort, Action Undo, Action Reload, Action<Bucket> Recolor);
