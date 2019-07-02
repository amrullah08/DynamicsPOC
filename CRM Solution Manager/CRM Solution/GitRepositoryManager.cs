//-----------------------------------------------------------------------
// <copyright file="GitRepositoryManager.cs" company="Microsoft">
//     Copyright (c) Microsoft. All rights reserved.
// </copyright>
// <author>Syed Amrullah Mazhar</author>
//-----------------------------------------------------------------------

namespace GitDeploy
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using CrmSolution;
    using LibGit2Sharp;
    using LibGit2Sharp.Handlers;

    /// <summary>
    /// Class for repository management
    /// </summary>
    public class GitRepositoryManager : IGitRepositoryManager
    {

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

        /// <summary>
        /// Commits all changes
        /// </summary>
        /// <param name="solutionFileInfo">solution file info</param>
        /// <param name="solutionFilePath">release solution list file</param>
        public void CommitAllChanges(SolutionFileInfo solutionFileInfo, string solutionFilePath)
        {
            try
            {
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" Committing Powershell Scripts" + "<br>");
                Console.WriteLine("Committing Powershell Scripts");
                string multilpleSolutionsImportPSPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Singleton.CrmConstantsInstance.MultilpleSolutionsImport);
                string multilpleSolutionsImportPSPathVirtual = this.localFolder + Singleton.CrmConstantsInstance.MultilpleSolutionsImport;
                File.Copy(multilpleSolutionsImportPSPath, multilpleSolutionsImportPSPathVirtual, true);

                string solutionToBeImportedPSPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Singleton.CrmConstantsInstance.SolutionToBeImported);
                string solutionToBeImportedPSPathVirtual = this.localFolder + Singleton.CrmConstantsInstance.SolutionToBeImported;
                File.Copy(solutionToBeImportedPSPath, solutionToBeImportedPSPathVirtual, true);

                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" Committing solutions" + "<br>");
                Console.WriteLine("Committing solutions");
                string fileUnmanaged = this.solutionlocalFolder + solutionFileInfo.SolutionUniqueName + "_.zip";
                File.Copy(solutionFileInfo.SolutionFilePath, fileUnmanaged, true);

                string fileManaged = this.solutionlocalFolder + solutionFileInfo.SolutionUniqueName + "_managed_.zip";
                File.Copy(solutionFileInfo.SolutionFilePathManaged, fileManaged, true);

                string webResources = solutionFileInfo.SolutionExtractionPath + "\\WebResources";

                using (var repo = new Repository(this.localFolder.FullName))
                {
                    AddRepositoryIndexes(fileUnmanaged, repo);
                    AddRepositoryIndexes(fileManaged, repo);

                    this.AddWebResourcesToRepository(webResources, repo);

                    // todo: add extracted solution files to repository
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

                    commitIds += string.Format("Commit Info <br/><a href='{0}/commit/{1}'>{2}</a>", this.repoUrl, commit.Id.Sha, commit.Message);
                    solutionFileInfo.Solution[Constants.SourceControlQueueAttributeNameForCommitIds] = commitIds;
                }
            }
            catch (EmptyCommitException ex)
            {
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" " + ex.Message + "<br>");
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

                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" Pushing Changes to the Repository " + "<br>");
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
                    Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" " + e.Message + "<br>");
                    Console.WriteLine(e.Message);
                }

                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" " + "Pushed changes" + "<br>");
                Console.WriteLine("Pushed changes");
            }
        }

        /// <summary>
        /// Adds indexes for repository
        /// </summary>
        /// <param name="file">Export solution location</param>
        /// <param name="repo">Repository</param>
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
                            repo.Index.Add(f.Replace(this.localFolder.FullName, string.Empty));
                        }
                        else
                        {
                            if (f.EndsWith(file))
                            {
                                repo.Index.Add(f.Replace(this.localFolder.FullName, string.Empty));
                            }
                        }
                    }
                }
            }
            else
            {
                repo.Index.Add(file.Replace(this.localFolder.FullName, string.Empty));
            }
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
                                MergeOptions = new MergeOptions()
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
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" " + r.Message + "<br>");
                        Console.WriteLine(r.Message);
                    }
                }
                else
                {
                    try
                    {
                        DirectoryInfo attachments_AR = new DirectoryInfo(workingDirectory);
                        EmptyFolder(attachments_AR);
                    }
                    catch (Exception ex)
                    {
                        Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" " + ex.Message + "<br>");
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
                Singleton.SolutionFileInfoInstance.webJobLogs.AppendLine(" One possibility for the exception could be check for branch or incorrect branch" + branchName + "<br>");
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
        public void CopyDirectory(string source, string destination, Repository repo)
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
                    this.CopyDirectory(element, destination + Path.GetFileName(element), repo);
                }
                else
                {
                    // Files in directory
                    File.Copy(element, destination + Path.GetFileName(element), true);
                    repo.Index.Add((destination + Path.GetFileName(element)).Replace(this.localFolder.FullName, string.Empty));
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
                EmptyFolder(subdirectory);
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

            foreach (var dataFile in Directory.GetFiles(webResources, "*.data.xml"))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(dataFile);
                var webResourceType = xmlDoc.SelectSingleNode("//WebResource/WebResourceType").InnerText;
                var webResournceName = xmlDoc.SelectSingleNode("//WebResource/Name").InnerText;
                string commitFileLoc = null;
                switch (webResourceType)
                {
                    case "1":
                        commitFileLoc = this.webResourcesHtmllocalFolder + webResournceName;
                        break;

                    case "3":
                        commitFileLoc = this.webResourcesJsFolder + webResournceName;
                        break;

                    case "5":
                        commitFileLoc = this.webResourcesImageslocalFolder + webResournceName;
                        break;
                }

                File.Copy(webResources + "\\" + webResournceName, commitFileLoc, true);
                repo.Index.Add(commitFileLoc.Replace(this.localFolder.FullName, string.Empty));
            }
        }

        /// <summary>
        /// Method adds extracted solution resources to repository
        /// </summary>
        /// <param name="solutionFileInfo">Solution file information</param>
        /// <param name="repo">repository to be committed</param>
        private void AddExtractedSolutionToRepository(SolutionFileInfo solutionFileInfo, Repository repo)
        {
            this.CopyDirectory(solutionFileInfo.SolutionExtractionPath, this.solutionlocalFolder.FullName + solutionFileInfo.SolutionUniqueName, repo);
        }
    }
}