using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NWebDav.Server.Dispatching;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Storage;
using SecureFolderFS.Core.WebDav;
using SecureFolderFS.Core.WebDav.AppModels;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.SystemStorageEx;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.Uno.Helpers;

namespace SecureFolderFS.Uno.Platforms.Desktop
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    internal sealed partial class SkiaWebDavFileSystem
    {
#if HAS_UNO_SKIA && !__MACCATALYST__ && !__UNO_SKIA_MACOS__FALSE
        /// <inheritdoc/>
        public override Task<string> GetVolumeNameAsync(string candidateName, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(candidateName);
        }

        /// <inheritdoc/>
        protected override async Task<IVfsRoot> MountAsync(
            FileSystemSpecifics specifics,
            HttpListener listener,
            WebDavOptions options,
            IRequestDispatcher requestDispatcher,
            CancellationToken cancellationToken)
        {
            var remotePath = DriveMappingHelpers.GetRemotePath(options.Protocol, options.Domain, options.Port, options.VolumeName);
            var mountPath = await DriveMappingHelpers.GetMountPathForRemotePathAsync(remotePath);

            var webDavWrapper = new WebDavWrapper(listener, requestDispatcher, mountPath);
            webDavWrapper.StartFileSystem();

            // return new WebDavVfsRoot(webDavWrapper, new SystemFolderEx(remotePath, options.VolumeName), specifics);
            // TODO: FIX. THIS LINE WILL CRASH BECAUSE REMOTE PATH IS URL
            var virtualizedRoot = new SystemFolderEx(remotePath);

            var plaintextRoot = new CryptoFolder(
                Path.DirectorySeparatorChar.ToString(),
                specifics.ContentFolder,
                specifics);

            return new WebDavVfsRoot(
                webDavWrapper,
                virtualizedRoot,
                plaintextRoot,
                specifics);
        }
#endif
    }
}
