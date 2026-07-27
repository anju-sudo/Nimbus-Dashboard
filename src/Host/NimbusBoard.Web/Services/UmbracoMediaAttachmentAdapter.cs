using NimbusBoard.Application.Common.Interfaces;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Nimbus_Board.Services;

/// <summary>
/// Stores issue attachments in Umbraco Media library.
/// </summary>
public class UmbracoMediaAttachmentAdapter(
    IMediaService mediaService,
    MediaFileManager mediaFileManager,
    MediaUrlGeneratorCollection mediaUrlGenerators,
    IShortStringHelper shortStringHelper,
    IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
    LocalFileAttachmentStorage localFallback) : IAttachmentStorage
{
    private const string FolderName = "Nimbus Board Attachments";

    public async Task<int> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = GetOrCreateFolder();
            var media = mediaService.CreateMedia(fileName, folder.Id, Constants.Conventions.MediaTypes.File);

            using var memory = new MemoryStream();
            await fileStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;

            media.SetValue(
                mediaFileManager,
                mediaUrlGenerators,
                shortStringHelper,
                contentTypeBaseServiceProvider,
                Constants.Conventions.Media.File,
                fileName,
                memory);
            mediaService.Save(media);
            return media.Id;
        }
        catch
        {
            fileStream.Position = 0;
            return await localFallback.SaveAsync(fileStream, fileName, cancellationToken);
        }
    }

    public string GetMediaUrl(int mediaId)
    {
        if (mediaId >= 10000)
        {
            return localFallback.GetMediaUrl(mediaId);
        }

        var media = mediaService.GetById(mediaId);
        if (media is null)
        {
            return "#";
        }

        var url = media.GetUrl(Constants.Conventions.Media.File, mediaUrlGenerators);
        return string.IsNullOrEmpty(url) ? $"/media/{mediaId}/" : url;
    }

    public async Task DeleteAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        if (mediaId >= 10000)
        {
            await localFallback.DeleteAsync(mediaId, cancellationToken);
            return;
        }

        var media = mediaService.GetById(mediaId);
        if (media is not null)
        {
            mediaService.Delete(media);
        }
    }

    private IMedia GetOrCreateFolder()
    {
        var root = mediaService.GetRootMedia().FirstOrDefault();
        var parentId = root?.Id ?? -1;

        var folder = mediaService.GetPagedChildren(parentId, 0, int.MaxValue, out _)
            .FirstOrDefault(m => m.Name == FolderName);

        if (folder is not null)
        {
            return folder;
        }

        folder = mediaService.CreateMedia(FolderName, parentId, Constants.Conventions.MediaTypes.Folder);
        mediaService.Save(folder);
        return folder;
    }
}
