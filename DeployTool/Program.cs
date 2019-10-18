using Amazon;
using Amazon.GameLift;
using Amazon.GameLift.Model;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeployTool
{
    class Program
    {
        static RegionEndpoint region = Amazon.RegionEndpoint.USEast1; // default
        static string bucket = null;

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            string name = null;
            string version = null;
            string rootpath = null;
            bool alias = false; // default
            string aliasUpdate = null;
            bool aliasop = false;

            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (String.Equals(args[i], @"--name", StringComparison.OrdinalIgnoreCase))
                    {
                        if (name != null) throw new ArgumentException("Error: --name may only be specified once");
                        if (++i > args.Length) throw new ArgumentException("Error: --name should be followed by a <name>");
                        name = args[i];
                        if (name.StartsWith("--")) throw new ArgumentException("Error: --name should be followed by a name");
                        if (name.Length > 40) throw new ArgumentException("Error: name is too long");
                        continue;
                    }

                    if (String.Equals(args[i], @"--version", StringComparison.OrdinalIgnoreCase))
                    {
                        if (version != null) throw new ArgumentException("Error: --version may only be specified once");
                        if (++i > args.Length) throw new ArgumentException("Error: --version should be followed by a <version>");
                        version = args[i];
                        if (version.StartsWith("--")) throw new ArgumentException("Error: --version should be followed by a <version>");
                        if (version.Length > 15) throw new ArgumentException("Error: version is too long");
                        continue;
                    }

                    if (String.Equals(args[i], @"--root-path", StringComparison.OrdinalIgnoreCase))
                    {
                        if (rootpath != null) throw new ArgumentException("Error: --root-path may only be specified once");
                        if (++i > args.Length) throw new ArgumentException("Error: --root-path should be followed by a <rootpath>");
                        rootpath = args[i];
                        if (rootpath.StartsWith("--")) throw new ArgumentException("Error: --root-path should be followed by a <rootpath>");
                        if (rootpath.Length > 1024) throw new ArgumentException("Error: root-path is too long");
                        continue;
                    }

                    if (String.Equals(args[i], @"--region", StringComparison.OrdinalIgnoreCase))
                    {
                        bool valid = false;
                        if (++i > args.Length) throw new ArgumentException("Error: --region should be followed by a <region>");
                        foreach (RegionEndpoint r in RegionEndpoint.EnumerableAllRegions)
                        {
                            if (String.Equals(args[i], r.SystemName, StringComparison.OrdinalIgnoreCase))
                            {
                                region = r;
                                valid = true;
                            }
                        }
                        if (!valid) throw new ArgumentException("Error: --region should be followed by a valid <region> system name, e.g. us-east-1");
                        continue;
                    }

                    if (String.Equals(args[i], @"--alias", StringComparison.OrdinalIgnoreCase))
                    {
                        if (aliasop) throw new ArgumentException("Error: only one alias operation allowed, either --alias, --no-alias, or --update-alias");
                        aliasop = true;
                        alias = true;
                        continue;
                    }

                    if (String.Equals(args[i], @"--noalias", StringComparison.OrdinalIgnoreCase))
                    {
                        if (aliasop) throw new ArgumentException("Error: only one alias operation allowed, either --alias, --no-alias, or --update-alias");
                        aliasop = true;
                        alias = false;
                        continue;
                    }

                    if (String.Equals(args[i], @"--no-alias", StringComparison.OrdinalIgnoreCase))
                    {
                        if (aliasop) throw new ArgumentException("Error: only one alias operation allowed, either --alias, --no-alias, or --update-alias");
                        aliasop = true;
                        alias = false;
                        continue;
                    }

                    if (String.Equals(args[i], @"--update-alias", StringComparison.OrdinalIgnoreCase))
                    {
                        if (aliasop) throw new ArgumentException("Error: only one alias operation allowed, either --alias, --no-alias, or --update-alias");
                        aliasop = true;
                        if (++i > args.Length) throw new ArgumentException("Error: --update-alias should be followed by an <aliasid>");
                        if (Regex.Match(args[i], @"^alias-[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase).Success)
                        {
                            aliasUpdate = args[i].ToLowerInvariant();
                        }
                        else
                        {
                            throw new ArgumentException("Error: --update-alias should be followed by a valid alias id, e.g. alias-3edb1432-14c8-4650-b350-aaf7090fd5a2");
                        }
                        continue;
                    }

                    if (String.Equals(args[i], @"--help", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException("Help:");
                    }

                    if (args[i].StartsWith("--")) throw new ArgumentException("Error: Unknown option \"" + args[i] + "\"");
                }
                if (name == null) throw new ArgumentException("Error: --name <name> is mandatory");
                if (version == null) throw new ArgumentException("Error: --version <version> is mandatory");
                if (rootpath == null) throw new ArgumentException("Error: --root-path <rootpath> is mandatory");

                await DeployBuild(name, version, rootpath, alias, aliasUpdate);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine();
                Console.WriteLine(@"Usage Examples:");
                Console.WriteLine(@"DeployTool --help");
                Console.WriteLine(@"DeployTool --name myfleet --version 1.0.1 --root-path C:\proj");
                Console.WriteLine(@"DeployTool --name FleetName --version 1.24x --root-path C:\proj --alias");
                Console.WriteLine(@"DeployTool --region us-west-2 --name FleetName --version six --root-path C:\proj");
                Console.WriteLine();
                Console.WriteLine(@"Grammar:");
                Console.WriteLine(@"DeployTool <help>|<options>");
                Console.WriteLine(@"<options>  ::= <name>|<ver>|<root>|<region>|<aliasopt> [<options>]");
                Console.WriteLine(@"<help>     ::= --help                show this message");
                Console.WriteLine(@"<name>     ::= --name <string>       fleet name *");
                Console.WriteLine(@"<ver>      ::= --version <string>    fleet version *");
                Console.WriteLine(@"<root>     ::= --root-path <path>    image root *");
                Console.WriteLine(@"<region>   ::= --region <sys-name>   deploy to region (default us-east-1)");
                Console.WriteLine(@"<aliasopt> ::= <alias>|<update>");
                Console.WriteLine(@"<alias>    ::= --alias|--no-alias    also create an alias to fleet (or not)");
                Console.WriteLine(@"<update>   ::= --update-alias <aliasid>");
                Console.WriteLine(@"<aliasid>  ::= alias-[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}");
                Console.WriteLine(@"<sys-name> ::= us-east-1|us-east-2|us-west-1|us-west-2|ap-south-1|");
                Console.WriteLine(@"               ap-northeast-2|ap-southeast-1|ap-southeast-2|ap-northeast-1|");
                Console.WriteLine(@"               ca-central-1|cn-north-1|eu-central-1|eu-west-1|eu-west-2|");
                Console.WriteLine(@"               sa-east-1");
                Console.WriteLine();
                Console.WriteLine(@"* name, ver and root options are mandatory for fleet creation");
                Console.WriteLine(@"  option order is not important; no spaces in arguments");
                Console.WriteLine(@"  case insensitive except for rootpath value on linux fleets");
                Console.WriteLine();
            }
        }

        static private async Task DeployBuild(string name, string version, string rootpath, bool alias, string aliasUpdate)
        {
            // Get AWS account number to make our bucket name unique
            var awsAccountId = GetAWSNum();
            
            // Bucket
            bucket = "buildbucket-" + region.SystemName + "-" + awsAccountId;

            // Create the Role for GameLift to use S3 (if it doesn't already exist)
            string roleName = "GameLiftS3Access-"+region.SystemName; // the role for GameLift to use S3
            string roleArn = await GetRoleArn(roleName);
            string policy = @"{""Version"": ""2012-10-17"",""Statement"": [{""Action"": [""s3:GetObject"",""s3:GetObjectVersion""],""Resource"": ""arn:aws:s3:::"
                + bucket + @"/*"",""Effect"": ""Allow""}]}";
            if (roleArn == null) roleArn = await CreateGameLiftRole(roleName, policy);

            // Zip and upload the build to a unique bucket
            DeleteZip();
            string zipfile = ZipBuild(rootpath);
            await CreateBucket();
            var s3Location = UploadBuild(zipfile, roleArn);

            // Create the build from the S3 bucket and the fleet
            string build = await CreateBuild(name, version, s3Location);
            string fleetId = await CreateFleet(name, version, build);
            if (alias)
            {
                string aliasId = await CreateAlias(name, fleetId);
                Console.WriteLine(aliasId);
            }
            else if (aliasUpdate != null)
            {
                string aliasId = await UpdateAlias(aliasUpdate, fleetId);
                Console.WriteLine(aliasId);
            }
            else
            {
                Console.WriteLine(fleetId);
            }
        }

        private static string GetAWSNum()
        {
            try
            {
                var config = new AmazonIdentityManagementServiceConfig();
                config.RegionEndpoint = region;
                using (var aimsc = new AmazonIdentityManagementServiceClient(config))
                {
                    var response = aimsc.GetUser();

                    string arn = response.User.Arn;
                    return arn.Split(':')[4];

                }
            }
            catch (NoSuchEntityException) // role was not present
            {
                return null;
            }
            catch (AmazonIdentityManagementServiceException imsException)
            {
                Console.WriteLine(imsException.Message, imsException.InnerException);
                throw;
            }
        }

        static string ZipBuild(string rootpath)
        {
            var zipfile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "build.zip");
            ZipFile.CreateFromDirectory(rootpath, zipfile);
            return zipfile;
        }

        static void DeleteZip()
        {
            var zipfile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "build.zip");
            File.Delete(zipfile);
        }

        static async Task<string> GetRoleArn(string roleName)
        {
            try
            {
                var config = new AmazonIdentityManagementServiceConfig();
                config.RegionEndpoint = region;
                using (var aimsc = new AmazonIdentityManagementServiceClient(config))
                {
                    var response = await aimsc.GetRoleAsync(new GetRoleRequest
                    {
                        RoleName = roleName
                    });

                    Role role = response.Role;
                    return role.Arn;
                }
            }
            catch (NoSuchEntityException) // role was not present
            {
                return null;
            }
            catch (AmazonIdentityManagementServiceException imsException)
            {
                Console.WriteLine(imsException.Message, imsException.InnerException);
                throw;
            }
        }

        static async Task<string> CreateGameLiftRole(string roleName, string policy)
        {
            try
            {
                var config = new AmazonIdentityManagementServiceConfig();
                config.RegionEndpoint = region;
                using (var aimsc = new AmazonIdentityManagementServiceClient(config))
                {
                    string assumeRole = @"{""Version"":""2012-10-17"",""Statement"":[{""Effect"":""Allow"",""Principal"":{""Service"": ""gamelift.amazonaws.com""},""Action"":""sts:AssumeRole""}]}";

                    var crres = await aimsc.CreateRoleAsync(new CreateRoleRequest
                    {
                        AssumeRolePolicyDocument = assumeRole,
                        Path = "/",
                        RoleName = roleName
                    });

                    Role role = crres.Role;

                    var cpres = await aimsc.CreatePolicyAsync(new CreatePolicyRequest
                    {
                        PolicyName = roleName + "Policy",
                        Description = "This allows GameLift to access services",
                        PolicyDocument = policy,
                        Path = "/"
                    });

                    var policyArn = cpres.Policy.Arn;

                    var response = await aimsc.AttachRolePolicyAsync(new AttachRolePolicyRequest
                    {
                        PolicyArn = policyArn,
                        RoleName = roleName
                    });

                    return role.Arn;
                }
            }
            catch (AmazonIdentityManagementServiceException imsException)
            {
                Console.WriteLine(imsException.Message, imsException.InnerException);
                throw;
            } 
        }

        static Amazon.GameLift.Model.S3Location UploadBuild(string zipfile, string roleArn)
        {
            try
            {
                TransferUtility fileTransferUtility =
                    new TransferUtility(new AmazonS3Client(region));

                fileTransferUtility.Upload(zipfile, bucket, "build.zip");
            }
            catch (AmazonS3Exception s3Exception)
            {
                Console.WriteLine(s3Exception.Message, s3Exception.InnerException);
                throw;
            }
            var s3Location = new Amazon.GameLift.Model.S3Location();
            s3Location.Bucket = bucket;
            s3Location.Key = "build.zip";
            s3Location.RoleArn = roleArn;
            return s3Location;
        }

        public static async Task CreateBucket()
        {
            using (var client = new AmazonS3Client(region))
            {

                if (!(await AmazonS3Util.DoesS3BucketExistAsync(client, bucket)))
                {
                    await CreateBucket(client);
                }
                for (int x = 0; !(await AmazonS3Util.DoesS3BucketExistAsync(client, bucket)); x++)
                {
                    Thread.Sleep(1);
                    if (x == 30000) throw new Exception("Bucket " + bucket + " was successfully created but still could not be found after 30 seconds. Wait a few minutes and try again or check S3");
                }
            }
        }

        static async Task<string> FindBucketLocation(IAmazonS3 client)
        {
            string bucketLocation;
            GetBucketLocationRequest request = new GetBucketLocationRequest()
            {
                BucketName = bucket
            };
            GetBucketLocationResponse response = await client.GetBucketLocationAsync(request);
            bucketLocation = response.Location.ToString();
            return bucketLocation;
        }

        static async Task CreateBucket(IAmazonS3 client)
        {
            try
            {
                PutBucketRequest putRequest1 = new PutBucketRequest
                {
                    BucketName = bucket
                };

                PutBucketResponse response1 = await client.PutBucketAsync(putRequest1);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine("For service sign up go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                        "Error occurred. Message:'{0}' when writing an object"
                        , amazonS3Exception.Message);
                }
            }
        }

        static async Task<string> CreateBuild(string name, string version, Amazon.GameLift.Model.S3Location s3Location)
        {
            using (var aglc = new AmazonGameLiftClient(new AmazonGameLiftConfig
            {
                RegionEndpoint = region
            }))
            {
                try
                {
                    CreateBuildResponse cbres = await aglc.CreateBuildAsync(new CreateBuildRequest
                    {
                        Name = name,
                        Version = version,
                        StorageLocation = s3Location
                    });
                    return cbres.Build.BuildId;
                }
                catch (AmazonGameLiftException glException)
                {
                    Console.WriteLine(glException.Message, glException.InnerException);
                    throw;
                }
            }
        }

        static async Task<string> CreateFleet(string name, string version, string buildId)
        {
            var config = new AmazonGameLiftConfig();
            config.RegionEndpoint = region;
            using (AmazonGameLiftClient aglc = new AmazonGameLiftClient(config))
            {
                // create launch configuration
                var serverProcess = new ServerProcess();
                serverProcess.ConcurrentExecutions = 1;
                serverProcess.LaunchPath = @"C:\game\GameLiftUnity.exe"; // @"/local/game/ReproGLLinux.x86_64";
                serverProcess.Parameters = "-batchmode -nographics";

                // create inbound IP permissions
                var permission1 = new IpPermission();
                permission1.FromPort = 1935;
                permission1.ToPort = 1935;
                permission1.Protocol = IpProtocol.TCP;
                permission1.IpRange = "0.0.0.0/0";

                // create inbound IP permissions
                var permission2 = new IpPermission();
                permission2.FromPort = 3389;
                permission2.ToPort = 3389;
                permission2.Protocol = IpProtocol.TCP;
                permission2.IpRange = "0.0.0.0/0";

                // create fleet
                var cfreq = new CreateFleetRequest();
                cfreq.Name = name;
                cfreq.Description = version;
                cfreq.BuildId = buildId;
                cfreq.EC2InstanceType = EC2InstanceType.C4Large;
                cfreq.EC2InboundPermissions.Add(permission1);
                cfreq.EC2InboundPermissions.Add(permission2);
                cfreq.RuntimeConfiguration = new RuntimeConfiguration();
                cfreq.RuntimeConfiguration.ServerProcesses = new List<ServerProcess>();
                cfreq.RuntimeConfiguration.ServerProcesses.Add(serverProcess);
                cfreq.NewGameSessionProtectionPolicy = ProtectionPolicy.NoProtection;
                CreateFleetResponse cfres = await aglc.CreateFleetAsync(cfreq);

                // set fleet capacity
                var ufcreq = new UpdateFleetCapacityRequest();
                ufcreq.MinSize = 0;
                ufcreq.DesiredInstances = 1;
                ufcreq.MaxSize = 1;
                ufcreq.FleetId = cfres.FleetAttributes.FleetId;
                UpdateFleetCapacityResponse ufcres = await aglc.UpdateFleetCapacityAsync(ufcreq);

                // set scaling rule (switch fleet off after 1 day of inactivity)
                // If [MetricName] is [ComparisonOperator] [Threshold] for [EvaluationPeriods] minutes, then [ScalingAdjustmentType] to/by [ScalingAdjustment].
                var pspreq = new PutScalingPolicyRequest();
                pspreq.Name = "Switch fleet off after 1 day of inactivity";
                pspreq.MetricName = MetricName.ActiveGameSessions;
                pspreq.ComparisonOperator = ComparisonOperatorType.LessThanOrEqualToThreshold;
                pspreq.Threshold = 0.0; // double (don't use int)
                pspreq.EvaluationPeriods = 1435; // just under 1 day, 1435 appears to be the maximum permitted value now.
                pspreq.ScalingAdjustmentType = ScalingAdjustmentType.ExactCapacity;
                pspreq.ScalingAdjustment = 0;
                pspreq.FleetId = cfres.FleetAttributes.FleetId;
                PutScalingPolicyResponse pspres = await aglc.PutScalingPolicyAsync(pspreq);

                return cfres.FleetAttributes.FleetId;
            }
        }

        static async Task<string> CreateAlias(string name, string fleetId)
        {
            var config = new AmazonGameLiftConfig();
            config.RegionEndpoint = region;
            using (AmazonGameLiftClient aglc = new AmazonGameLiftClient(config))
            {
                // create a new alias
                var careq = new Amazon.GameLift.Model.CreateAliasRequest();
                careq.Name = name;
                careq.Description = "Alias to direct traffic to " + fleetId;
                careq.RoutingStrategy = new RoutingStrategy
                {
                    Type = RoutingStrategyType.SIMPLE,
                    FleetId = fleetId,
                    Message = ""
                };
                Amazon.GameLift.Model.CreateAliasResponse cares = await aglc.CreateAliasAsync(careq);

                return cares.Alias.AliasId;
            }
        }

        private static async Task<string> UpdateAlias(string aliasUpdate, string fleetId)
        {
            var config = new AmazonGameLiftConfig();
            config.RegionEndpoint = region;
            using (AmazonGameLiftClient aglc = new AmazonGameLiftClient(config))
            {
                // create a new alias
                var uareq = new Amazon.GameLift.Model.UpdateAliasRequest();
                uareq.AliasId = aliasUpdate;
                uareq.RoutingStrategy = new RoutingStrategy
                {
                    Type = RoutingStrategyType.SIMPLE,
                    FleetId = fleetId,
                    Message = ""
                };
                Amazon.GameLift.Model.UpdateAliasResponse cares = await aglc.UpdateAliasAsync(uareq);

                return cares.Alias.AliasId;
            }
        }
    }
}
