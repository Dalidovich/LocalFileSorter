namespace LocalFileSorter.Common.Localization;

public sealed class Strings
{
    public Strings(LocalizationCatalog catalog)
    {
        AppTitle = catalog.Resolve("app.title");

        StartupBanner = catalog.Resolve("startup.banner");
        StartupPromptSource = catalog.Resolve("startup.promptSource");
        StartupPromptDestination = catalog.Resolve("startup.promptDestination");
        StartupQuitHint = catalog.Resolve("startup.quitHint");
        StartupAborted = catalog.Resolve("startup.aborted");
        StartupOpening = catalog.Resolve("startup.opening");
        StartupMissingKeys = catalog.Resolve("startup.missingKeys");
        StartupMissingThemeTokens = catalog.Resolve("startup.missingThemeTokens");
        StartupScanSummary = catalog.Resolve("startup.scanSummary");
        StartupBucketSummary = catalog.Resolve("startup.bucketSummary");

        ValidationPathRequired = catalog.Resolve("validation.pathRequired");
        ValidationPathNotFound = catalog.Resolve("validation.pathNotFound");
        ValidationPathNotReadable = catalog.Resolve("validation.pathNotReadable");
        ValidationPathNotWritable = catalog.Resolve("validation.pathNotWritable");
        ValidationRootsEqual = catalog.Resolve("validation.rootsEqual");
        ValidationDestinationInsideSource = catalog.Resolve("validation.destinationInsideSource");

        PanelPreview = catalog.Resolve("panel.preview");
        PanelQueue = catalog.Resolve("panel.queue");
        PanelBuckets = catalog.Resolve("panel.buckets");

        PreviewNoFile = catalog.Resolve("preview.noFile");
        PreviewLoading = catalog.Resolve("preview.loading");
        PreviewPrevious = catalog.Resolve("preview.previous");
        PreviewNext = catalog.Resolve("preview.next");
        PreviewPosition = catalog.Resolve("preview.position");
        PreviewNoModule = catalog.Resolve("preview.noModule");
        PreviewModuleFailed = catalog.Resolve("preview.moduleFailed");
        PreviewUnreadable = catalog.Resolve("preview.unreadable");

        QueueEmpty = catalog.Resolve("queue.empty");
        QueueHidden = catalog.Resolve("queue.hidden");
        QueueHiddenExtensions = catalog.Resolve("queue.hiddenExtensions");
        QueueNoExtension = catalog.Resolve("queue.noExtension");
        QueueComplete = catalog.Resolve("queue.complete");

        BucketsEmpty = catalog.Resolve("buckets.empty");
        BucketsEmptyInstruction = catalog.Resolve("buckets.emptyInstruction");
        BucketsCount = catalog.Resolve("buckets.count");
        BucketsSort = catalog.Resolve("buckets.sort");
        BucketsSortPending = catalog.Resolve("buckets.sortPending");
        BucketsReloadMapping = catalog.Resolve("buckets.reloadMapping");
        BucketsUndo = catalog.Resolve("buckets.undo");
        BucketsRecolorTitle = catalog.Resolve("buckets.recolorTitle");
        BucketsRecolorSummary = catalog.Resolve("buckets.recolorSummary");

        MappingNoticeTitle = catalog.Resolve("mapping.noticeTitle");
        MappingNoticeUnchanged = catalog.Resolve("mapping.noticeUnchanged");
        MappingNoticeAdded = catalog.Resolve("mapping.noticeAdded");
        MappingNoticeRemoved = catalog.Resolve("mapping.noticeRemoved");
        MappingNoticeReleased = catalog.Resolve("mapping.noticeReleased");
        MappingNoticeCommitted = catalog.Resolve("mapping.noticeCommitted");

        CommitConfirmTitle = catalog.Resolve("commit.confirmTitle");
        CommitConfirmSummary = catalog.Resolve("commit.confirmSummary");
        CommitConfirmRow = catalog.Resolve("commit.confirmRow");
        CommitConfirmMore = catalog.Resolve("commit.confirmMore");
        CommitConfirmStart = catalog.Resolve("commit.confirmStart");
        CommitProgressTitle = catalog.Resolve("commit.progressTitle");
        CommitProgressCount = catalog.Resolve("commit.progressCount");
        CommitReportTitle = catalog.Resolve("commit.reportTitle");
        CommitReportMoved = catalog.Resolve("commit.reportMoved");
        CommitReportRenamed = catalog.Resolve("commit.reportRenamed");
        CommitReportFailed = catalog.Resolve("commit.reportFailed");
        CommitReportSkipped = catalog.Resolve("commit.reportSkipped");
        CommitReportCancelled = catalog.Resolve("commit.reportCancelled");
        CommitReportClean = catalog.Resolve("commit.reportClean");
        CommitFailureRow = catalog.Resolve("commit.failureRow");
        CommitReasonAlreadyMoved = catalog.Resolve("commit.reasonAlreadyMoved");
        CommitReasonSourceMissing = catalog.Resolve("commit.reasonSourceMissing");
        CommitReasonBucketUnavailable = catalog.Resolve("commit.reasonBucketUnavailable");
        CommitReasonNoFreeName = catalog.Resolve("commit.reasonNoFreeName");
        CommitReasonLocked = catalog.Resolve("commit.reasonLocked");
        CommitReasonDenied = catalog.Resolve("commit.reasonDenied");
        CommitReasonMoveFailed = catalog.Resolve("commit.reasonMoveFailed");

        MetaCreated = catalog.Resolve("meta.created");
        MetaModified = catalog.Resolve("meta.modified");
        MetaSize = catalog.Resolve("meta.size");
        MetaType = catalog.Resolve("meta.type");

        ImageResolution = catalog.Resolve("image.resolution");
        ImageResolutionValue = catalog.Resolve("image.resolutionValue");
        ImageDecodeFailed = catalog.Resolve("image.decodeFailed");
        ImageTooLarge = catalog.Resolve("image.tooLarge");

        TextLines = catalog.Resolve("text.lines");
        TextEncoding = catalog.Resolve("text.encoding");
        TextTruncated = catalog.Resolve("text.truncated");
        TextTruncationNotice = catalog.Resolve("text.truncationNotice");

        CommonYes = catalog.Resolve("common.yes");
        CommonNo = catalog.Resolve("common.no");
        CommonCancel = catalog.Resolve("common.cancel");
        CommonClose = catalog.Resolve("common.close");

        FormatDateTime = catalog.Resolve("format.dateTime");
        SizeBytes = catalog.Resolve("size.bytes");
        SizeKilobytes = catalog.Resolve("size.kilobytes");
        SizeMegabytes = catalog.Resolve("size.megabytes");
        SizeGigabytes = catalog.Resolve("size.gigabytes");
    }

    public string AppTitle { get; }

    public string StartupBanner { get; }
    public string StartupPromptSource { get; }
    public string StartupPromptDestination { get; }
    public string StartupQuitHint { get; }
    public string StartupAborted { get; }
    public string StartupOpening { get; }
    public string StartupMissingKeys { get; }
    public string StartupMissingThemeTokens { get; }
    public string StartupScanSummary { get; }
    public string StartupBucketSummary { get; }

    public string ValidationPathRequired { get; }
    public string ValidationPathNotFound { get; }
    public string ValidationPathNotReadable { get; }
    public string ValidationPathNotWritable { get; }
    public string ValidationRootsEqual { get; }
    public string ValidationDestinationInsideSource { get; }

    public string PanelPreview { get; }
    public string PanelQueue { get; }
    public string PanelBuckets { get; }

    public string PreviewNoFile { get; }
    public string PreviewLoading { get; }
    public string PreviewPrevious { get; }
    public string PreviewNext { get; }
    public string PreviewPosition { get; }
    public string PreviewNoModule { get; }
    public string PreviewModuleFailed { get; }
    public string PreviewUnreadable { get; }

    public string QueueEmpty { get; }
    public string QueueHidden { get; }
    public string QueueHiddenExtensions { get; }
    public string QueueNoExtension { get; }
    public string QueueComplete { get; }

    public string BucketsEmpty { get; }
    public string BucketsEmptyInstruction { get; }
    public string BucketsCount { get; }
    public string BucketsSort { get; }
    public string BucketsSortPending { get; }
    public string BucketsReloadMapping { get; }
    public string BucketsUndo { get; }
    public string BucketsRecolorTitle { get; }
    public string BucketsRecolorSummary { get; }

    public string MappingNoticeTitle { get; }
    public string MappingNoticeUnchanged { get; }
    public string MappingNoticeAdded { get; }
    public string MappingNoticeRemoved { get; }
    public string MappingNoticeReleased { get; }
    public string MappingNoticeCommitted { get; }

    public string CommitConfirmTitle { get; }
    public string CommitConfirmSummary { get; }
    public string CommitConfirmRow { get; }
    public string CommitConfirmMore { get; }
    public string CommitConfirmStart { get; }
    public string CommitProgressTitle { get; }
    public string CommitProgressCount { get; }
    public string CommitReportTitle { get; }
    public string CommitReportMoved { get; }
    public string CommitReportRenamed { get; }
    public string CommitReportFailed { get; }
    public string CommitReportSkipped { get; }
    public string CommitReportCancelled { get; }
    public string CommitReportClean { get; }
    public string CommitFailureRow { get; }
    public string CommitReasonAlreadyMoved { get; }
    public string CommitReasonSourceMissing { get; }
    public string CommitReasonBucketUnavailable { get; }
    public string CommitReasonNoFreeName { get; }
    public string CommitReasonLocked { get; }
    public string CommitReasonDenied { get; }
    public string CommitReasonMoveFailed { get; }

    public string MetaCreated { get; }
    public string MetaModified { get; }
    public string MetaSize { get; }
    public string MetaType { get; }

    public string ImageResolution { get; }
    public string ImageResolutionValue { get; }
    public string ImageDecodeFailed { get; }
    public string ImageTooLarge { get; }

    public string TextLines { get; }
    public string TextEncoding { get; }
    public string TextTruncated { get; }
    public string TextTruncationNotice { get; }

    public string CommonYes { get; }
    public string CommonNo { get; }
    public string CommonCancel { get; }
    public string CommonClose { get; }

    public string FormatDateTime { get; }
    public string SizeBytes { get; }
    public string SizeKilobytes { get; }
    public string SizeMegabytes { get; }
    public string SizeGigabytes { get; }
}
