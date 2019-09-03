//-----------------------------------------------------------------------
// <copyright file="GitRepositoryManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace GitDeploy
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using CrmSolution;
    using LibGit2Sharp;
    using LibGit2Sharp.Handlers;
    using Microsoft.TeamFoundation.Client;
    using Microsoft.TeamFoundation.VersionControl.Client;
    using Microsoft.TeamFoundation.VersionControl.Common;
    using Microsoft.VisualStudio.Services.Common;

    /// <summary>
    /// Class for repository management
    /// </summary>
    public class GitRepositoryManager : IGitRepositoryManager
    {
        private TfsTeamProjectCollection _tfs;
        private VersionControlServer _versionControl;
        private Workspace _workspace;

        /// <summary>
        /// tfs user name
        /// </summary>
        private readonly string tfsUserName;

        /// <summary>
        /// GitHub password
        /// </summary>
        private readonly string tfsPassword;

        /// <summary>
        /// Repository url
        /// </summary>
        private readonly string repoUrl;

        /// <summary>
        /// Author name for the commit
        /// </summary>
        private readonly string authorName;

        /// <summary>
        /// Author Email for the commit
        /// </summary>
        private readonly string authorEmail;

        /// <summary>
        /// Committer name
        /// </summary>
        private readonly string committerName;

        /// <summary>
        /// Committer email
        /// </summary>
        private readonly string committerEmail;

        /// <summary>
        /// remote name
        /// </summary>
        private readonly string remoteName;

        /// <summary>
        /// Branch name
        /// </summary>
        private readonly string branchName;

        /// <summary>
        /// User name and password credentials
        /// </summary>
        private readonly UsernamePasswordCredentials credentials;

        /// <summary>
        /// local repository folder
        /// </summary>
        private readonly DirectoryInfo localFolder;

        /// <summary>
        /// local java script folder
        /// </summary>
        private readonly DirectoryInfo webResourcesJsFolder;

        /// <summary>
        /// local Images folder
        /// </summary>
        private readonly DirectoryInfo webResourcesImageslocalFolder;

        /// <summary>
        /// local html folder
        /// </summary>
        private readonly DirectoryInfo webResourcesHtmllocalFolder;

        /// <summary>
        /// solution local folder
        /// </summary>
        private readonly DirectoryInfo solutionlocalFolder;

        /// <summary>
        /// Every time repository will be cloned
        /// </summary>
        private bool cloneAlways;

        /// <summary>
        /// Initializes a new instance of the <see cref="GitRepositoryManager" /> class.
        /// </summary>
        /// <param name="username">The credentials username.</param>
        /// <param name="password">The credentials password.</param>
        /// <param name="tfsUserName">The tfs credentials username.</param>
        /// <param name="tfsPassword">The tfs credentials password.</param>
        /// <param name="gitRepoUrl">The repo URL.</param>
        /// <param name="remoteName">remote name</param>
        /// <param name="branchName">branch name</param>
        /// <param name="cloneAlways">clone always</param>
        /// <param name="localFolder">The full path to local folder.</param>
        /// <param name="javascriptLocalFolder">The full path to java script local folder.</param>
        /// <param name="htmlLocalFolder">The full path to html local folder.</param>
        /// <param name="imagesLocalFolder">The full path to images local folder.</param>
        /// <param name="solutionLocalFolder">The full path to solution local folder.</param>
        /// <param name="authorname">author name</param>
        /// <param name="authoremail">author email</param>
        /// <param name="committername">committer name</param>
        /// <param name="committeremail">committer email</param>
        public GitRepositoryManager(
            string username,
            string password,
            string tfsUsername,
            string tfsPassWord,
            string gitRepoUrl,
            string remoteName,
            string branchName,
            bool cloneAlways,
            string localFolder,
            string javascriptLocalFolder,
            string htmlLocalFolder,
            string imagesLocalFolder,
            string solutionLocalFolder,
            string authorname,
            string authoremail,
            string committername,
            string committeremail)
        {
            var folder = new DirectoryInfo(localFolder);
            this.localFolder = folder;
            this.webResourcesJsFolder = new DirectoryInfo(javascriptLocalFolder);
            this.webResourcesHtmllocalFolder = new DirectoryInfo(htmlLocalFolder);
            this.webResourcesImageslocalFolder = new DirectoryInfo(imagesLocalFolder);
            this.solutionlocalFolder = new DirectoryInfo(solutionLocalFolder);

            this.credentials = new UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            };

            this.tfsUserName = tfsUsername;
            this.tfsPassword = tfsPassWord;
            this.repoUrl = gitRepoUrl;
            this.authorName = authorname;
            this.authorEmail = authoremail;
            this.committerName = committername;
            this.committerEmail = committeremail;
            this.remoteName = remoteName;
            this.branchName = branchName;
            this.cloneAlways = cloneAlways;
        }

        /// <summary>
        /// method checks if directory is empty or not
        /// </summary>
        /// <param name="path">directory path</param>
        /// <returns>method returns if directory is empty</returns>
        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        public void CommitFilesToGitHub(SolutionFileInfo solutionFileInfo, string solutionFilePath, string fileUnmanaged, string fileManaged,
          string webResources, string multilpleSolutionsImportPSPathVirtual, string solutionToBeImportedPSPathVirtual)
        {
            try
            {
                using (var repo = new Repository(this.localFolder.FullName))
                {
                    this.AddRepositoryIndexes(fileUnmanaged, repo);
                    this.AddRepositoryIndexes(fileManaged, repo);
                    this.AddWebResourcesToRepository(webResources, repo);
                    //// todo: add extracted solution files to repository
                    this.AddExtractedSolutionToRepository(solutionFileInfo, repo);

                    repo.Index.Add(solutionFilePath.Replace(this.localFolder.FullName, string.Empty));
                    repo.Index.Add(multilpleSolutionsImportPSPathVirtual.Replace(this.localFolder.FullName, string.Empty));
                    repo.Index.Add(solutionToBeImportedPSPathVirtual.Replace(this.localFolder.FullName, string.Empty));
                    var offset = DateTimeOffset.Now;
                    Signature author = new Signature(this.authorName, this.authorEmail, offset);
                    Signature committer = new Signature(this.committerName, this.committerEmail, offset);
                    CommitOptions commitOptions = new CommitOptions
                    {
                        AllowEmptyCommit = false
                    };

                    var commit = repo.Commit(solutionFileInfo.Message, author, committer);
                    string commitIds = solutionFileInfo.Solution.GetAttributeValue<string>(Constants.SourceControlQueueAttributeNameForCommitIds);
                    if (string.IsNullOrEmpty(commitIds))
                    {
                        commitIds = string.Empty;
                    }

                    commitIds += string.Format("Commit Info <br/><a href='{0}/commit/{1}'>{2}</a>", this.repoUrl.Replace(".git", ""), commit.Id.Sha, commit.Message);
                    solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForCommitIds] = commitIds;
                }
            }
            catch (EmptyCommitException ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
            }
        }

        public void CommitFilesToTFS(SolutionFileInfo solutionFileInfo, string solutionFilePath, string fileUnmanaged, string fileManaged,
        string webResources, string multilpleSolutionsImportPSPathVirtual, string solutionToBeImportedPSPathVirtual)
        {
            try
            {
                this.AddRepositoryIndexes(fileUnmanaged, null);
                this.AddRepositoryIndexes(fileManaged, null);
                this.AddWebResourcesToRepository(webResources, null);
                //// todo: add extracted solution files to repository
                this.AddExtractedSolutionToRepository(solutionFileInfo, null);
                List<PendingChange> pc = new List<PendingChange>(_workspace.GetPendingChanges());

                PendingChange[] pcArr = pc.Where(x => x.ChangeType != ChangeType.Delete && x.ItemType != ItemType.Folder && x.FileName != this.localFolder.FullName.ToString()).ToArray();

                if (pcArr.Length > 0)
                {
                    _workspace.CheckIn(pcArr, "Automated Checkin (" + _workspace + ") " + DateTime.Today.ToShortDateString());
                    Console.WriteLine("Completed: Checkin");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" <br/> Completed: Checkin");
                }

                foreach (var pchg in pc)
                {
                    Console.WriteLine(pchg.ChangeType.ToString() + "::" + pchg.FileName);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("<br/>" + pchg.ChangeType.ToString() + "::" + pchg.FileName);

                }

                WorkingFolder workfolder = new WorkingFolder(this.branchName, this.localFolder.FullName.ToString());
                _workspace.Undo(this.localFolder.FullName.ToString(), RecursionType.Full);
                _workspace.DeleteMapping(workfolder);
                _workspace.Delete();
                Console.WriteLine("After CheckIn workspace deleted");


                solutionFileInfo.Update();
                solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForStatus] = Constants.SourceControlQueuemPushToRepositorySuccessStatus;
                solutionFileInfo.Solution.Attributes["syed_webjobs"] = Singleton.SolutionFileInfoInstance.WebJobs();

                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + "Successfully Pushed files to TFS" + "<br>");
                Console.WriteLine("Successfully Pushed files to TFS");
                Singleton.SolutionFileInfoInstance.UploadFiletoDynamics(Singleton.CrmConstantsInstance.ServiceProxy, solutionFileInfo.Solution);

            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
                bool folderExists = Directory.Exists(this.localFolder.FullName);
                if (folderExists)
                {
                    var fList = Directory.GetFiles(this.localFolder.FullName, "*.*", SearchOption.AllDirectories);
                    foreach (var f in fList)
                    {
                        File.Delete(f);
                    }
                }
                if (_workspace != null)
                {
                    _workspace.Delete();
                }

            }
        }

        /// <summary>
        /// Method populates hash set from source control release file
        /// </summary>
        /// <param name="solutionFilePath">path of file that contains list of solution to be released</param>
        /// <param name="hashSet">hash set to store release solution</param>
        private void PopulateHashset(string solutionFilePath, HashSet<string> hashSet)
        {
            try
            {
                if (File.Exists(solutionFilePath))
                {
                    string[] lines = File.ReadAllLines(solutionFilePath);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            hashSet.Add(line);
                        }
                    }
                }
                else
                {
                    File.Create(solutionFilePath).Dispose();
                }
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Commits all changes
        /// </summary>
        /// <param name="solutionFileInfo">solution file info</param>
        /// <param name="solutionFilePath">release solution list file</param>
        public void CommitAllChanges(SolutionFileInfo solutionFileInfo, string solutionFilePath, HashSet<string> hashSet)
        {
            try
            {
                string multilpleSolutionsImportPSPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\MultilpleSolutionsImport.ps1";
                string multilpleSolutionsImportPSPathVirtual = this.localFolder + Singleton.CrmConstantsInstance.MultilpleSolutionsImport;
                string solutionToBeImportedPSPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\SolutionToBeImported.ps1";
                string solutionToBeImportedPSPathVirtual = this.localFolder + Singleton.CrmConstantsInstance.SolutionToBeImported;
                string fileUnmanaged = this.solutionlocalFolder + solutionFileInfo.SolutionUniqueName + "_.zip";
                string fileManaged = this.solutionlocalFolder + solutionFileInfo.SolutionUniqueName + "_managed_.zip";
                string webResources = solutionFileInfo.SolutionExtractionPath + "\\WebResources";

                if (this._workspace == null && solutionFileInfo.Repository != Constants.SourceControlAzureDevOpsServer)
                {

                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Committing Powershell Scripts" + "<br>");
                    Console.WriteLine("Committing Powershell Scripts");
                    File.Copy(multilpleSolutionsImportPSPath, multilpleSolutionsImportPSPathVirtual, true);
                    File.Copy(solutionToBeImportedPSPath, solutionToBeImportedPSPathVirtual, true);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Committing solutions" + "<br>");
                    Console.WriteLine("Committing solutions");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy managed" + "<br>");
                    File.Copy(solutionFileInfo.SolutionFilePathManaged, fileManaged, true);
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy unmanaged" + "<br>");
                    File.Copy(solutionFileInfo.SolutionFilePath, fileUnmanaged, true);
                    CommitFilesToGitHub(solutionFileInfo, solutionFilePath, fileUnmanaged, fileManaged, webResources, multilpleSolutionsImportPSPathVirtual, solutionToBeImportedPSPathVirtual);
                }
                else
                {
                    bool solutionTXTFiles = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(this.localFolder + "solutions.flat"), ItemType.Any);
                    if (solutionTXTFiles)
                    {
                        var tes = new string[1];
                        tes[0] = this.localFolder + "solutions.flat";
                        _workspace.PendEdit(tes, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                        // File.WriteAllText(this.localFolder + "solutions.flat", String.Empty);

                        if (solutionFileInfo.SolutionsTxt == 433710000 && File.Exists(this.localFolder + "solutions.flat"))
                        {
                            File.WriteAllText(this.localFolder + "solutions.flat", string.Empty);
                            hashSet.Clear();
                        }

                        this.PopulateHashset(solutionFilePath, hashSet);
                        if (!hashSet.Contains(solutionFileInfo.SolutionFileZipName) && solutionFileInfo.IncludeInRelease)
                        {
                            hashSet.Add(solutionFileInfo.SolutionFileZipName);
                        }

                        using (var tw = new StreamWriter(this.localFolder + "solutions.flat", true))
                        {
                            var numLines = hashSet.Where(cc => !string.IsNullOrEmpty(cc)).ToArray();
                            foreach (var item in numLines)
                            {
                                tw.WriteLine(item);
                            }
                            tw.Close();
                        }
                    }
                    else
                    {
                        File.Create(solutionFilePath);
                        File.WriteAllText(solutionFilePath, string.Empty);
                        File.WriteAllLines(solutionFilePath, hashSet.Where(cc => !string.IsNullOrEmpty(cc)).ToArray());
                        _workspace.PendAdd(solutionFilePath, true);
                    }

                    //Copy Zip Files
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Committing solutions" + "<br>");
                    Console.WriteLine("Committing solutions");

                    bool solutionfileUnManagedExit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(fileUnmanaged), ItemType.Any);
                    if (solutionfileUnManagedExit)
                    {
                        var fileUnmanagedLocation = new string[1];
                        fileUnmanagedLocation[0] = fileUnmanaged;
                        _workspace.PendEdit(fileUnmanagedLocation, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy unmanaged" + "<br>");
                        File.Copy(solutionFileInfo.SolutionFilePath, fileUnmanaged, true);
                    }
                    else
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy unmanaged" + "<br>");
                        File.Copy(solutionFileInfo.SolutionFilePath, fileUnmanaged, true);
                        _workspace.PendAdd(fileUnmanaged, true);
                    }


                    bool solutionfileManagedExit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(fileManaged), ItemType.Any);
                    if (solutionfileManagedExit)
                    {
                        var fileManagedLocation = new string[1];
                        fileManagedLocation[0] = fileManaged;
                        _workspace.PendEdit(fileManagedLocation, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy managed" + "<br>");
                        File.Copy(solutionFileInfo.SolutionFilePathManaged, fileManaged, true);
                    }
                    else
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Copy managed" + "<br>");
                        File.Copy(solutionFileInfo.SolutionFilePathManaged, fileManaged, true);
                        _workspace.PendAdd(fileManaged, true);
                    }

                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Committing Powershell Scripts" + "<br>");
                    Console.WriteLine("Committing Powershell Scripts");

                    //Copy PowerShell
                    bool fileExit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(multilpleSolutionsImportPSPathVirtual), ItemType.Any);
                    if (fileExit)
                    {
                        var multilpleSolutionsImportPSPathVirtualLocatioin = new string[1];
                        multilpleSolutionsImportPSPathVirtualLocatioin[0] = multilpleSolutionsImportPSPathVirtual;
                        _workspace.PendEdit(multilpleSolutionsImportPSPathVirtualLocatioin, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                        File.Copy(multilpleSolutionsImportPSPath, multilpleSolutionsImportPSPathVirtual, true);

                    }
                    else
                    {
                        File.Copy(multilpleSolutionsImportPSPath, multilpleSolutionsImportPSPathVirtual, true);
                        _workspace.PendAdd(multilpleSolutionsImportPSPathVirtual, true);
                    }

                    //Copy PowerShell

                    bool exits = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(solutionToBeImportedPSPathVirtual), ItemType.Any);
                    if (exits)
                    {
                        var solutionToBeImportedPSPathVirtualLocation = new string[1];
                        solutionToBeImportedPSPathVirtualLocation[0] = solutionToBeImportedPSPathVirtual;
                        _workspace.PendEdit(solutionToBeImportedPSPathVirtualLocation, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                        File.Copy(solutionToBeImportedPSPath, solutionToBeImportedPSPathVirtual, true);
                    }
                    else
                    {
                        File.Copy(solutionToBeImportedPSPath, solutionToBeImportedPSPathVirtual, true);
                        _workspace.PendAdd(solutionToBeImportedPSPathVirtual, true);
                    }

                    CommitFilesToTFS(solutionFileInfo, solutionFilePath, fileUnmanaged, fileManaged, webResources,
                        multilpleSolutionsImportPSPathVirtual, solutionToBeImportedPSPathVirtual);
                }
            }
            catch (Exception ex)
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Pushes all commits.
        /// </summary>
        /// <exception cref="System.Exception">Writes exception to the console</exception>
        public void PushCommits()
        {
            string remoteName = this.remoteName, branchName = this.branchName;
            using (var repo = new Repository(this.localFolder.FullName))
            {
                var remote = repo.Network.Remotes.FirstOrDefault(r => r.Name == remoteName);
                if (remote == null)
                {
                    repo.Network.Remotes.Add(remoteName, this.repoUrl);
                    remote = repo.Network.Remotes.FirstOrDefault(r => r.Name == remoteName);
                }

                var options = new PushOptions
                {
                    CredentialsProvider = (url, usernameFromUrl, types) => this.credentials
                };

                options.OnPushStatusError += this.PushSatusErrorHandler;

                string pushRefs = "refs/heads/testsyed";
                Branch branchs = null;
                foreach (var branch in repo.Branches)
                {
                    if (branch.FriendlyName.ToLower().Equals(branchName.ToLower()))
                    {
                        branchs = branch;
                        pushRefs = branch.Reference.CanonicalName;
                    }
                }

                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" Pushing Changes to the Repository " + "<br>");
                Console.WriteLine(" Pushing Changes to the Repository ");

                repo.Network.Push(remote, pushRefs + ":" + pushRefs, options);
                try
                {
                    var remoteOrigin = repo.Network.Remotes.FirstOrDefault(r => r.Name == "remotes/origin" + "<br>");

                    if (remoteOrigin != null)
                    {
                        repo.Network.Push(remoteOrigin, pushRefs, options);
                    }
                }
                catch (Exception e)
                {
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + e.Message + "<br>");
                    Console.WriteLine(e.Message);
                }

                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + "Pushed changes" + "<br>");
                Console.WriteLine("Pushed changes");
            }
        }

        /// <summary>
        /// Method updates repository
        /// </summary>
        public Workspace ConnectTFSMap(SolutionFileInfo solutionFileInfo, string solutionFilePath, HashSet<string> hashSet)
        {
            try
            {
                NetworkCredential networkCredential = new NetworkCredential(this.tfsUserName, this.tfsPassword);
                VssBasicCredential basicCredential = new VssBasicCredential(networkCredential);
                VssCredentials tfsCredentials = new VssCredentials(basicCredential);

                _tfs = new TfsTeamProjectCollection(new Uri(this.repoUrl), tfsCredentials);
                _tfs.Authenticate();

                // Get a reference to Version Control.              
                _versionControl = _tfs.GetService<VersionControlServer>();
                _versionControl.NonFatalError += GitRepositoryManager.OnNonFatalError;
                _versionControl.Getting += GitRepositoryManager.OnGetting;
                _versionControl.BeforeCheckinPendingChange += GitRepositoryManager.OnBeforeCheckinPendingChange;
                _versionControl.NewPendingChange += GitRepositoryManager.OnNewPendingChange;

                _workspace = _versionControl.TryGetWorkspace(this.localFolder.FullName);
                if (_workspace != null)
                {
                    WorkingFolder workfolderToDelete = new WorkingFolder(this.branchName, this.localFolder.FullName);
                    if (_workspace.MappingsAvailable)
                    {
                        _workspace.Undo(this.localFolder.FullName, RecursionType.Full);
                        _workspace.DeleteMapping(workfolderToDelete);
                    }
                    _workspace.Delete();
                    Console.WriteLine("deleted workspace;");
                    Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" <br/> deleted workspace;");
                }


                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" <br/>" + _versionControl.AuthorizedUser);

                Console.WriteLine("workspace create");
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("workspace create <br/>");
                CreateWorkspaceParameters parameters = new CreateWorkspaceParameters(Guid.NewGuid().ToString());
                parameters.Location = WorkspaceLocation.Server;

                _workspace = _versionControl.CreateWorkspace(parameters);
                Console.WriteLine(" workspace created;");
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("workspace created <br/>");
                WorkingFolder workfolder = new WorkingFolder(this.branchName, this.localFolder.FullName);
                // Create a mapping using the Team Project supplied on the command line.
                _workspace.CreateMapping(workfolder);

                Console.WriteLine("Completed: Map;");

                // Get the files from the repository.
                GetRequest request = new GetRequest(new ItemSpec(this.localFolder.FullName, RecursionType.Full), VersionSpec.Latest);

                _workspace.Get();
                Console.WriteLine("Got Files");

                Console.WriteLine("Completed: Files Mapped ");
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine("Completed: Files Mapped  <br/>");
                Console.WriteLine("CheckOut Files");

                this.CommitAllChanges(solutionFileInfo, solutionFilePath, hashSet);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(ex.Message);
                throw ex;
            }
            return _workspace;
        }

        internal static void OnNonFatalError(Object sender, ExceptionEventArgs e)
        {
            if (e.Exception != null)
            {
                Console.Error.WriteLine("  Non - fatal exception: " + e.Exception.Message);
            }
            else
            {
                Console.Error.WriteLine("  Non - fatal failure: " + e.Failure.Message);
            }
        }
        internal static void OnGetting(Object sender, GettingEventArgs e)
        {
            Console.WriteLine("  Getting: " + e.TargetLocalItem + ", status: " + e.Status);
        }
        internal static void OnBeforeCheckinPendingChange(Object sender, ProcessingChangeEventArgs e)
        {
            Console.WriteLine("  Checking in " + e.PendingChange.LocalItem);
        }
        internal static void OnNewPendingChange(Object sender, PendingChangeEventArgs e)
        {
            Console.WriteLine("  Pending " + PendingChange.GetLocalizedStringForChangeType(e.PendingChange.ChangeType) + " on " + e.PendingChange.LocalItem);
        }

        /// <summary>
        /// Method updates repository
        /// </summary>
        public void UpdateRepository()
        {
            //// https://github.com/libgit2/libgit2sharp/blob/2f8ad262b7613e9e68aefd4f55956bf24b05d042/LibGit2Sharp.Tests/MergeFixture.cs
            //// https://github.com/libgit2/libgit2sharp/blob/2f8ad262b7613e9e68aefd4f55956bf24b05d042/LibGit2Sharp.Tests/FetchFixture.cs#L129-L153

            string remoteName = this.remoteName, branchName = this.branchName;
            bool cloneAlways = this.cloneAlways;
            string url = this.repoUrl;
            Credentials crednt(string curl, string usernameFromUrl, SupportedCredentialTypes types) => this.credentials;

            string workingDirectory = this.localFolder.FullName;
            if (Directory.Exists(workingDirectory))
            {
                if (!cloneAlways)
                {
                    try
                    {
                        using (var repo = new Repository(workingDirectory))
                        {
                            repo.Reset(ResetMode.Hard);
                            repo.RemoveUntrackedFiles();

                            var remote = repo.Network.Remotes.FirstOrDefault(r => r.Name == remoteName);

                            string pushRefs = "refs/heads/testsyed";
                            Branch branchs = null;
                            foreach (var branch in repo.Branches)
                            {
                                if (branch.FriendlyName.ToLower().Equals(branchName.ToLower()))
                                {
                                    branchs = branch;
                                    pushRefs = branch.Reference.CanonicalName;
                                }
                            }

                            var fetchOptions = new FetchOptions() { CredentialsProvider = crednt };
                            repo.Network.Fetch(remote.Name, new string[] { pushRefs }, fetchOptions);

                            PullOptions pullOptions = new PullOptions()
                            {
                                MergeOptions = new LibGit2Sharp.MergeOptions()
                                {
                                    FastForwardStrategy = FastForwardStrategy.Default,
                                },
                                FetchOptions = fetchOptions
                            };

                            MergeResult mergeResult = Commands.Pull(
                                repo,
                                new Signature(this.authorName, this.authorEmail, DateTimeOffset.Now),
                                pullOptions);
                        }

                        return;
                    }
                    catch (LibGit2Sharp.RepositoryNotFoundException r)
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + r.Message + "<br>");
                        Console.WriteLine(r.Message);
                    }
                }
                else
                {
                    try
                    {
                        DirectoryInfo attachments_AR = new DirectoryInfo(workingDirectory);
                        this.EmptyFolder(attachments_AR);
                    }
                    catch (Exception ex)
                    {
                        Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" " + ex.Message + "<br>");
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(workingDirectory);
            }

            var cloneOptions = new CloneOptions
            {
                CredentialsProvider = crednt
            };

            cloneOptions.BranchName = branchName;

            try
            {
                Repository.Clone(url, workingDirectory, cloneOptions);
                this.cloneAlways = false;
            }
            catch
            {
                Singleton.SolutionFileInfoInstance.WebJobsLog.AppendLine(" One possibility for the exception could be check for branch or incorrect branch" + branchName + "<br>");
                Console.WriteLine("One possibility for the exception could be check for branch or incorrect branch" + branchName);
                throw;
            }
        }

        /// <summary>
        /// method copies one directory to another
        /// </summary>
        /// <param name="source">source directory</param>
        /// <param name="destination">destination directory</param>
        /// <param name="repo">repository instance</param>
        public void CopyDirectory(string source, string destination, Repository repo, Workspace _workspace)
        {
            string[] files;

            if (destination[destination.Length - 1] != Path.DirectorySeparatorChar)
            {
                destination += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            files = Directory.GetFileSystemEntries(source);
            foreach (string element in files)
            {
                // Sub directories
                if (Directory.Exists(element))
                {
                    if (repo != null)
                    {
                        this.CopyDirectory(element, destination + Path.GetFileName(element), repo, null);
                    }
                    else
                    {
                        this.CopyDirectory(element, destination + Path.GetFileName(element), null, _workspace);
                    }

                }
                else
                {
                    if (repo != null)
                    {
                        // Files in directory
                        File.Copy(element, destination + Path.GetFileName(element), true);
                        File.Copy(element, destination + Path.GetFileName(element), true);

                        repo.Index.Add((destination + Path.GetFileName(element)).Replace(this.localFolder.FullName, string.Empty));
                    }
                    else
                    {
                        bool fileExit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(destination + Path.GetFileName(element)), ItemType.Any);
                        if (fileExit)
                        {
                            var tes = new string[1];
                            tes[0] = destination + Path.GetFileName(element);
                            _workspace.PendEdit(tes, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                            // Files in directory
                            File.Copy(element, destination + Path.GetFileName(element), true);
                            File.Copy(element, destination + Path.GetFileName(element), true);

                        }
                        else
                        {
                            // Files in directory
                            File.Copy(element, destination + Path.GetFileName(element), true);
                            File.Copy(element, destination + Path.GetFileName(element), true);
                            _workspace.PendAdd(destination + Path.GetFileName(element), true);
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Method empties folder
        /// </summary>
        /// <param name="directory">Directory to be emptied</param>
        private void EmptyFolder(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subdirectory in directory.GetDirectories())
            {
                this.EmptyFolder(subdirectory);
                subdirectory.Delete();
            }
        }

        /// <summary>
        /// Push status error handler
        /// </summary>
        /// <param name="pushStatusErrors">push status errors</param>
        private void PushSatusErrorHandler(PushStatusError pushStatusErrors)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method adds web resources to repository
        /// </summary>
        /// <param name="webResources">web resources folder</param>
        /// <param name="repo">repository to be committed</param>
        private void AddWebResourcesToRepository(string webResources, Repository repo)
        {
            if (!Directory.Exists(webResources))
            {
                return;
            }

            foreach (var dataFile in Directory.GetFiles(webResources, "*.data.xml", SearchOption.AllDirectories))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(dataFile);
                var webResourceType = xmlDoc.SelectSingleNode("//WebResource/WebResourceType").InnerText;
                string webResournceName = xmlDoc.SelectSingleNode("//WebResource/Name").InnerText;
                string modifiedName = string.Empty;
                string[] webList = Regex.Split(webResournceName, "/");
                if (webList.Length != 0)
                {
                    modifiedName = webList[webList.Length - 1];
                }
                if (modifiedName == "")
                {
                    modifiedName = webResournceName;
                }
                string commitFileLoc = null;
                switch (webResourceType)
                {
                    case "1":
                        commitFileLoc = this.webResourcesHtmllocalFolder + modifiedName;
                        break;

                    case "3":
                        commitFileLoc = this.webResourcesJsFolder + modifiedName;
                        break;

                    case "5":
                        commitFileLoc = this.webResourcesImageslocalFolder + modifiedName;
                        break;
                }
                if (commitFileLoc != null && commitFileLoc != string.Empty)
                {
                    if (repo != null)
                    {
                        File.Copy(webResources + "\\" + webResournceName, commitFileLoc, true);
                        repo.Index.Add(commitFileLoc.Replace(this.localFolder.FullName, string.Empty));
                    }
                    else
                    {
                        bool fileExit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(commitFileLoc), ItemType.Any);
                        if (fileExit)
                        {

                            var tes = new string[1];
                            tes[0] = commitFileLoc;
                            _workspace.PendEdit(tes, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                            File.Copy(webResources + "\\" + webResournceName, commitFileLoc, true);

                        }
                        else
                        {
                            File.Copy(webResources + "\\" + webResournceName, commitFileLoc, true);
                            _workspace.PendAdd(commitFileLoc, true);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Method adds extracted solution resources to repository
        /// </summary>
        /// <param name="solutionFileInfo">Solution file information</param>
        /// <param name="repo">repository to be committed</param>
        private void AddExtractedSolutionToRepository(SolutionFileInfo solutionFileInfo, Repository repo)
        {
            if (repo != null)
            {
                this.CopyDirectory(solutionFileInfo.SolutionExtractionPath, this.solutionlocalFolder.FullName + solutionFileInfo.SolutionUniqueName, repo, null);

            }
            else
            {
                this.CopyDirectory(solutionFileInfo.SolutionExtractionPath, this.solutionlocalFolder.FullName + solutionFileInfo.SolutionUniqueName, null, _workspace);

            }

        }

        /// <summary>
        /// Adds indexes for repository
        /// </summary>
        /// <param name="file">Export solution location</param>
        /// <param name="repo">Repository details</param>
        private void AddRepositoryIndexes(string file, Repository repo)
        {
            if (string.IsNullOrEmpty(file))
            {
                var files = this.solutionlocalFolder.GetFiles("*.zip").Select(f => f.FullName);
                {
                    foreach (var f in files)
                    {
                        if (string.IsNullOrEmpty(file))
                        {
                            if (repo != null)
                            {
                                repo.Index.Add(f.Replace(this.localFolder.FullName, string.Empty));
                            }
                            else
                            {
                                bool fileExit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(file), ItemType.Any);
                                if (fileExit)
                                {
                                    var tes = new string[1];
                                    tes[0] = file;
                                    _workspace.PendEdit(tes, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);

                                }
                                else
                                {
                                    _workspace.PendAdd(file, true);
                                }
                            }
                        }
                        else
                        {
                            if (f.EndsWith(file))
                            {
                                if (repo != null)
                                {
                                    repo.Index.Add(f.Replace(this.localFolder.FullName, string.Empty));
                                }
                                else
                                {
                                    bool fxit = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(file), ItemType.Any);
                                    if (fxit)
                                    {
                                        var tes = new string[1];
                                        tes[0] = file;
                                        _workspace.PendEdit(tes, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                                    }
                                    else
                                    {
                                        _workspace.PendAdd(file, true);
                                    }

                                }
                            }

                        }
                    }
                }
            }
            else
            {
                if (repo != null)
                {
                    repo.Index.Add(file.Replace(this.localFolder.FullName, string.Empty));
                }
                else
                {
                    bool isFile = _versionControl.ServerItemExists(_workspace.GetServerItemForLocalItem(file), ItemType.Any);
                    if (isFile)
                    {
                        var tes = new string[1];
                        tes[0] = file;
                        _workspace.PendEdit(tes, RecursionType.Full, null, LockLevel.None, false, PendChangesOptions.GetLatestOnCheckout);
                    }
                    else
                    {
                        _workspace.PendAdd(file, true);
                    }
                }
            }

        }
    }
}