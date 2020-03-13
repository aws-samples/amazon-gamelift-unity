// Copyright 2018 Amazon
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Amazon;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using Amazon.Runtime.CredentialManagement;

public class Credentials
{
    public static readonly string profileName = "demo-gamelift-unity";

    public static void MigrateProfile()
    {
        // Credential profile used to be stored in .net sdk credentials store.
        // Shared credentials file is more modern. Migrate old profile if needed.
        // Shows good form for profile management
        CredentialProfile profile;
        var scf = new SharedCredentialsFile();
        if (!scf.TryGetProfile(profileName, out _))
        {
            var nscf = new NetSDKCredentialsFile();
            if (nscf.TryGetProfile(profileName, out profile))
            {
                scf.RegisterProfile(profile);
                nscf.UnregisterProfile(profileName);
            }
        }
    }

    public static void Install()
    {
        MigrateProfile();

        // Use command line filename for credentials (*.csv file). As many as you like can be specified, only the first one found and valid will be used.
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] != "--credentials")
            {
                continue;
            }

            Debug.Log(":) LOADING CREDENTIALS STARTED. Install(): --credentials qualifier detected." + Environment.NewLine);

            if (!File.Exists(args[i + 1]))
            {
                Debug.Log(":( LOADING CREDENTIALS FAILED. Install(): Specified credentials file does not exist." + Environment.NewLine);
                continue;
            }

            string[] lines = File.ReadAllLines(args[i + 1]);
            if (lines.Length != 2)
            {
                Debug.Log(":( LOADING CREDENTIALS FAILED. Install(): Specified credentials file contains more or less than one set of credentials." + Environment.NewLine);
                continue;
            }

            string accessKey = null;
            string secretKey = null;
            string[] headers = lines[0].Split(',');
            string[] credentials = lines[1].Split(',');
            for (int idx = 0; idx < headers.Length; idx++)
            {
                if (headers[idx] == "Access key ID") accessKey = credentials[idx];
                if (headers[idx] == "Secret access key") secretKey = credentials[idx];
            }

            // Check Access Key
            string pattern1 = @"^[A-Z0-9]{20}$";
            Match m1 = Regex.Match(accessKey, pattern1);
            if (!m1.Success)
            {
                Debug.Log(":( LOADING CREDENTIALS FAILED. Install(): Specified credentials file contains invalid access key or no access key." + Environment.NewLine);
                continue;
            }

            // Check Secret Key
            string pattern2 = @"^[A-Za-z0-9/+=]{40}$";
            Match m2 = Regex.Match(secretKey, pattern2);
            if (!m2.Success)
            {
                Debug.Log(":( LOADING CREDENTIALS FAILED. Install(): Specified credentials file contains invalid secret key or no secret key." + Environment.NewLine);
                continue;
            }

            var options = new CredentialProfileOptions
            {
                AccessKey = accessKey,
                SecretKey = secretKey
            };
            var profile = new CredentialProfile(profileName, options);
            new SharedCredentialsFile().RegisterProfile(profile);
            Debug.Log(":) PROFILE REGISTERED SUCCESSFULLY IN SHARED CREDENTIALS FILE." + Environment.NewLine);
            break;
        }
    }
}