/*
* All or portions of this file Copyright (c) Amazon.com, Inc. or its affiliates or
* its licensors.
*
* For complete copyright and license terms please see the LICENSE at the root of this
* distribution (the "License"). All use of this software is governed by the License,
* or, if provided, by the license below or the license accompanying this file. Do not
* remove or modify any license notices. This file is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
*
*/

namespace Aws.GameLift
{
    public enum GameLiftErrorType
    {
        SERVICE_CALL_FAILED,
        LOCAL_CONNECTION_FAILED,
        NETWORK_NOT_INITIALIZED,
        GAMESESSION_ID_NOT_SET,
        TERMINATION_TIME_NOT_SET,
        BAD_REQUEST_EXCEPTION,
        INTERNAL_SERVICE_EXCEPTION
    };

    public class GameLiftError
    {
        public GameLiftErrorType ErrorType { get; set; }
        public string ErrorName { get; set; }
        public string ErrorMessage { get; set; }

        public GameLiftError()
        {
        }

        public GameLiftError(GameLiftErrorType errorType)
        {
            ErrorType = errorType;
            ErrorName = GetDefaultNameForErrorType(errorType);
            ErrorMessage = GetDefaultMessageForErrorType(errorType);
        }

        public GameLiftError(GameLiftErrorType errorType, string errorMessage)
        {
            ErrorType = errorType;
            ErrorName = GetDefaultNameForErrorType(errorType);
            ErrorMessage = errorMessage;
        }

        public GameLiftError(GameLiftErrorType errorType, string errorName, string errorMessage)
        {
            ErrorType = errorType;
            ErrorName = errorName;
            ErrorMessage = errorMessage;
        }

        string GetDefaultNameForErrorType(GameLiftErrorType errorType)
        {
            switch (errorType)
            {
                case GameLiftErrorType.SERVICE_CALL_FAILED:
                    return "Service call failed.";
                case GameLiftErrorType.LOCAL_CONNECTION_FAILED:
                    return "Local connection failed.";
                case GameLiftErrorType.NETWORK_NOT_INITIALIZED:
                    return "Network not initialized.";
                case GameLiftErrorType.GAMESESSION_ID_NOT_SET:
                    return "GameSession id is not set.";
                case GameLiftErrorType.TERMINATION_TIME_NOT_SET:
                    return "TerminationTime is not set.";
                case GameLiftErrorType.BAD_REQUEST_EXCEPTION:
                    return "Bad request exception.";
                case GameLiftErrorType.INTERNAL_SERVICE_EXCEPTION:
                    return "Internal service exception.";
                default:
                    return "Unknown Error";
            }
        }

        string GetDefaultMessageForErrorType(GameLiftErrorType errorType)
        {
            switch (errorType)
            {
                case GameLiftErrorType.SERVICE_CALL_FAILED:
                    return "An AWS service call has failed. See the root cause error for more information.";
                case GameLiftErrorType.LOCAL_CONNECTION_FAILED:
                    return "Connection to local agent could not be established.";
                case GameLiftErrorType.NETWORK_NOT_INITIALIZED:
                    return "Local network was not initialized. Have you called InitSDK()?";
                case GameLiftErrorType.GAMESESSION_ID_NOT_SET:
                    return "No game sessions are bound to this process.";
                case GameLiftErrorType.TERMINATION_TIME_NOT_SET:
                    return "TerminationTime has not been sent to this process.";
                case GameLiftErrorType.BAD_REQUEST_EXCEPTION:
                    return "Bad request exception.";
                case GameLiftErrorType.INTERNAL_SERVICE_EXCEPTION:
                    return "Internal service exception.";
                default:
                    return "An unexpected error has occurred.";
            }
        }

        public override string ToString()
        {
            return string.Format("[GameLiftError: ErrorType={0}, ErrorName={1}, ErrorMessage={2}]", ErrorType, ErrorName, ErrorMessage);
        }
    }
}
