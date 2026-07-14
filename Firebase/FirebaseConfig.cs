using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Auth;
using Google.Cloud.Firestore.V1;

namespace LibraryManagementSystem.Firebase
{
    /// <summary>
    /// Central bootstrap for Firestore access using the official Google.Cloud.Firestore SDK.
    /// Call FirebaseConfig.Initialize(...) once at application startup (see Program.cs).
    /// </summary>
    public static class FirebaseConfig
    {
        private static FirestoreDb _firestoreDb;

        public static string ProjectId { get; set; }
        public static string CredentialsPath { get; set; }

        public static void Initialize(string projectId, string credentialsPath)
        {
            ProjectId = projectId;
            CredentialsPath = credentialsPath;

            _firestoreDb = null;
        }

        public static FirestoreDb GetFirestoreDb()
        {
            if (_firestoreDb != null)
                return _firestoreDb;

            if (string.IsNullOrWhiteSpace(ProjectId) || string.IsNullOrWhiteSpace(CredentialsPath))
                throw new InvalidOperationException(
                    "FirebaseConfig.Initialize(projectId, credentialsPath) must be called before use.");

            if (!File.Exists(CredentialsPath))
                throw new FileNotFoundException(
                    $"Firebase service account key not found at '{CredentialsPath}'. " +
                    "Download it from Firebase Console > Project Settings > Service Accounts, " +
                    "and place it there (see README.md).", CredentialsPath);

            var credential = GoogleCredential.FromFile(CredentialsPath);

            var builder = new FirestoreClientBuilder
            {
                ChannelCredentials = credential.ToChannelCredentials()
            };

            _firestoreDb = FirestoreDb.Create(ProjectId, builder.Build());
            return _firestoreDb;
        }
    }
}
