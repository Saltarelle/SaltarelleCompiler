using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using NodeJS.BufferModule;

namespace NodeJS.FSModule {
	[Imported]
	[GlobalMethods]
	[ModuleName("fs")]
	public static class FS {
		public static void Rename(string oldPath, string newPath) {}
		public static void Rename(string oldPath, string newPath, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'rename', {oldPath}, {newPath})")]
		public static Task RenameTask(string oldPath, string newPath) { return null; }
		public static void RenameSync(string oldPath, string newPath) {}

		public static void Truncate(int fd, int len) {}
		public static void Truncate(int fd, int len, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'truncate', {fd}, {len})")]
		public static Task TruncateTask(int fd, int len) { return null; }
		public static void TruncateSync(int fd, int len) {}

		public static void Chown(string path, int uid, int gid) {}
		public static void Chown(string path, int uid, int gid, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'chown', {path}, {uid}, {gid})")]
		public static Task ChownTask(string path, int uid, int gid) { return null; }
		public static void ChownSync(string path, int uid, int gid) {}

		public static void FChown(int fd, int uid, int gid) {}
		public static void FChown(int fd, int uid, int gid, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'fchown', {fd}, {uid}, {gid})")]
		public static Task FChownTask(int fd, int uid, int gid) { return null; }
		public static void FChownSync(int fd, int uid, int gid) {}

		public static void LChown(string path, int uid, int gid) {}
		public static void LChown(string path, int uid, int gid, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'lchown', {path}, {uid}, {gid})")]
		public static Task LChownTask(string path, int uid, int gid) { return null; }
		public static void LChownSync(string path, int uid, int gid) {}

		public static void Chmod(string path, TypeOption<int, string> mode) {}
		public static void Chmod(string path, TypeOption<int, string> mode, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'chmod', {path}, {mode})")]
		public static Task ChmodTask(string path, TypeOption<int, string> mode) { return null; }
		public static void ChmodSync(string path, TypeOption<int, string> mode) {}

		public static void FChmod(int fd, TypeOption<int, string> mode) {}
		public static void FChmod(int fd, TypeOption<int, string> mode, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'fchmod', {fd}, {mode})")]
		public static Task FChmodTask(int fd, TypeOption<int, string> mode) { return null; }
		public static void FChmodSync(int fd, TypeOption<int, string> mode) {}

		public static void LChmod(string path, TypeOption<int, string> mode) {}
		public static void LChmod(string path, TypeOption<int, string> mode, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'lchmod', {path}, {mode})")]
		public static Task LChmodTask(string path, TypeOption<int, string> mode) { return null; }
		public static void LChmodSync(string path, TypeOption<int, string> mode) {}

		public static void Stat(string path, Action<Error, Stats> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'stat', {path})")]
		public static Task<Stats> StatTask(string path) { return null; }
		public static Stats StatSync(string path) { return null; }

		public static void FStat(int fd, Action<Error, Stats> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'fstat', {fd})")]
		public static Task<Stats> StatTask(int fd) { return null; }
		public static Stats StatSync(int fd) { return null; }

		public static void LStat(string path, Action<Error, Stats> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'lstat', {path})")]
		public static Task<Stats> LStatTask(string path) { return null; }
		public static Stats LStatSync(string path) { return null; }

		public static void Link(string srcpath, string dstpath) {}
		public static void Link(string srcpath, string dstpath, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'link', {srcpath}, {dstpath})")]
		public static Task LinkTask(string srcpath, string dstpath) { return null; }
		public static void LinkSync(string srcpath, string dstpath) {}

		public static void Symlink(string srcpath, string dstpath) {}
		public static void Symlink(string srcpath, string dstpath, SymlinkType type) {}
		public static void Symlink(string srcpath, string dstpath, Action<Error> callback) {}
		public static void Symlink(string srcpath, string dstpath, SymlinkType type, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'symlink', {srcpath}, {dstpath})")]
		public static Task SymlinkTask(string srcpath, string dstpath) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'symlink', {srcpath}, {dstpath}, {type})")]
		public static Task SymlinkTask(string srcpath, string dstpath, SymlinkType type) { return null; }
		public static void SymlinkSync(string srcpath, string dstpath) {}
		public static void SymlinkSync(string srcpath, string dstpath, SymlinkType type) {}

		public static void Readlink(string path, Action<Error, string> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'readlink', {path})")]
		public static Task<string> ReadlinkTask(string path) { return null; }
		public static string RealinkSync(string path) { return null; }

		public static void Realpath(string path, Action<Error, string> callback) {}
		public static void Realpath(string path, JsDictionary<string, string> cache, Action<Error, string> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'realpath', {path})")]
		public static Task<string> RealpathTask(string path) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'realpath', {path}, {cache})")]
		public static Task<string> RealpathTask(string path, JsDictionary<string, string> cache) { return null; }
		public static string RealpathSync(string path) { return null; }
		public static string RealpathSync(string path, JsDictionary<string, string> cache) { return null; }

		public static void Unlink(string path) {}
		public static void Unlink(string path, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'unlink', {path})")]
		public static Task UnlinkTask(string path) { return null; }
		public static void UnlinkSync(string path) {}

		public static void Rmdir(string path) {}
		public static void Rmdir(string path, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'rmdir', {path})")]
		public static Task RmdirTask(string path) { return null; }
		public static void RmdirSync(string path) {}

		public static void Mkdir(string path) {}
		public static void Mkdir(string path, TypeOption<int, string> mode) {}
		public static void Mkdir(string path, Action<Error> callback) {}
		public static void Mkdir(string path, TypeOption<int, string> mode, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'mkdir', {path})")]
		public static Task MkdirTask(string path) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'mkdir', {path}, {mode})")]
		public static Task MkdirTask(string path, TypeOption<int, string> mode) { return null; }
		public static void MkdirSync(string path) {}
		public static void MkdirSync(string path, TypeOption<int, string> mode) {}

		public static void Readdir(string path, Action<Error, string[]> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'readdir', {path})")]
		public static Task<string[]> ReaddirTask(string path) { return null; }
		public static string[] ReaddirSync(string path) { return null; }

		public static void Close(int fd) {}
		public static void Close(int fd, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'close', {fd})")]
		public static Task CloseTask(int fd) { return null; }
		public static void CloseSync(int fd) {}

		public static void Open(string path, OpenFlags flags, Action<Error, int> callback) {}
		public static void Open(string path, OpenFlags flags, TypeOption<int, string> mode, Action<Error, int> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'open', {path}, {flags})")]
		public static Task<int> OpenTask(string path, OpenFlags flags) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'open', {path}, {flags}, {mode})")]
		public static Task<int> OpenTask(string path, OpenFlags flags, TypeOption<int, string> mode) { return null; }
		public static int OpenSync(string path, OpenFlags flags) { return 0; }
		public static int OpenSync(string path, OpenFlags flags, TypeOption<int, string> mode) { return 0; }

		public static void UTimes(string path, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime) {}
		public static void UTimes(string path, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'utimes', {path}, {atime}, {mtime})")]
		public static Task UTimesTask(string path, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime) { return null; }
		public static void UTimesSync(string path, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime) {}

		public static void FUTimes(int fd, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime) {}
		public static void FUTimes(int fd, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'futimes', {fd}, {atime}, {mtime})")]
		public static Task FUTimesTask(int fd, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime) { return null; }
		public static void FUTimesSync(int fd, TypeOption<DateTime, int> atime, TypeOption<DateTime, int> mtime) {}

		public static void FSync(int fd) {}
		public static void FSync(int fd, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'fsync', {fd})")]
		public static Task FSyncTask(int fd) { return null; }
		public static Stats FSyncSync(int fd) { return null; }

		public static void Write(int fd, Buffer buffer, int offset, int length, int? position) {}
		public static void Write(int fd, Buffer buffer, int offset, int length, int? position, Action<Error, string, Buffer> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, function(w, b) {{ return {{ written: w, buffer: b }}; }}, 'write', {fd}, {buffer}, {offset}, {length}, {position})")]
		public static Task<WriteResult> WriteTask(int fd, Buffer buffer, int offset, int length, int? position) { return null; }
		public static int WriteSync(int fd, Buffer buffer, int offset, int length, int position) { return 0; }

		public static void Read(int fd, Buffer buffer, int offset, int length, int? position) {}
		public static void Read(int fd, Buffer buffer, int offset, int length, int? position, Action<Error, string, Buffer> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, function(r, b) {{ return {{ bytesRead: r, buffer: b }}; }}, 'read', {fd}, {buffer}, {offset}, {length}, {position})")]
		public static Task<ReadResult> ReadTask(int fd, Buffer buffer, int offset, int length, int? position) { return null; }
		public static int ReadSync(int fd, Buffer buffer, int offset, int length, int? position) { return 0; }

		public static void ReadFile(string filename, Action<Error, Buffer> callback) {}
		public static void ReadFile(string filename, Encoding encoding, Action<Error, string> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'readFile', {filename})")]
		public static Task<Buffer> ReadFileTask(string filename) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'readFile', {filename}, {encoding})")]
		public static Task<string> ReadFileTask(string filename, Encoding encoding) { return null; }
		public static Buffer ReadFileSync(string filename) { return null; }
		public static string ReadFileSync(string filename, Encoding encoding) { return null; }


		public static void WriteFile(string filename, string data) {}
		public static void WriteFile(string filename, string data, Encoding encoding) {}
		public static void WriteFile(string filename, Buffer data) {}
		public static void WriteFile(string filename, string data, Action<Error> callback) {}
		public static void WriteFile(string filename, string data, Encoding encoding, Action<Error> callback) {}
		public static void WriteFile(string filename, Buffer data, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'writeFile', {filename}, {data})")]
		public static Task WriteFileTask(string filename, string data) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'writeFile', {filename}, {data}, {encoding})")]
		public static Task WriteFileTask(string filename, string data, Encoding encoding) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'writeFile', {filename}, {data})")]
		public static Task WriteFileTask(string filename, Buffer data) { return null; }
		public static void WriteFileSync(string filename, string data) {}
		public static void WriteFileSync(string filename, string data, Encoding encoding) {}
		public static void WriteFileSync(string filename, Buffer data) {}

		public static void AppendFile(string filename, string data) {}
		public static void AppendFile(string filename, string data, Encoding encoding) {}
		public static void AppendFile(string filename, Buffer data) {}
		public static void AppendFile(string filename, string data, Action<Error> callback) {}
		public static void AppendFile(string filename, string data, Encoding encoding, Action<Error> callback) {}
		public static void AppendFile(string filename, Buffer data, Action<Error> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'appendFile', {filename}, {data})")]
		public static Task AppendFileTask(string filename, string data) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'appendFile', {filename}, {data}, {encoding})")]
		public static Task AppendFileTask(string filename, string data, Encoding encoding) { return null; }
		[InlineCode("{$System.Threading.Tasks.Task}.fromNode({$NodeJS.FSModule.FS}, 'appendFile', {filename}, {data})")]
		public static Task AppendFileTask(string filename, Buffer data) { return null; }
		public static void AppendFileSync(string filename, string data) {}
		public static void AppendFileSync(string filename, string data, Encoding encoding) {}
		public static void AppendFileSync(string filename, Buffer data) {}

		public static void WatchFile(string filename, Action<Stats, Stats> listener) {}
		public static void WatchFile(string filename, WatchOptions options, Action<Stats, Stats> listener) {}

		public static void UnwatchFile(string filename) {}
		public static void UnwatchFile(string filename, Action<Stats, Stats> listener) {}

		public static FSWatcher Watch(string filename) { return null; }
		public static FSWatcher Watch(string filename, Action<WatchEventType, string> listener) { return null; }
		public static FSWatcher Watch(string filename, WatchOptions options) { return null; }
		public static FSWatcher Watch(string filename, WatchOptions options, Action<WatchEventType, string> listener) { return null; }

		public static void Exists(string filename, Action<bool> callback) {}
		[InlineCode("{$System.Threading.Tasks.Task}.fromDoneCallback({$NodeJS.FSModule.FS}, 'exists', {filename})")]
		public static Task<bool> ExistsTask(string filename) { return null; }
		public static bool ExistsSync(string filename) { return false; }

		public static ReadStream CreateReadStream(string path) { return null; }
		public static ReadStream CreateReadStream(string path, CreateReadStreamOptions options) { return null; }

		public static WriteStream CreateWriteStream(string path) { return null; }
		public static WriteStream CreateWriteStream(string path, CreateWriteStreamOptions options) { return null; }
	}
}
