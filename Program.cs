using LibraryManagementSystem.Firebase;
using LibraryManagementSystem.Forms;

namespace LibraryManagementSystem
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // TODO: move these into Configurations/appsettings.json loading if you
            // want them configurable without recompiling.
            FirebaseConfig.Initialize(
                projectId: "library-management-9cf74",
                credentialsPath: Path.Combine(AppContext.BaseDirectory, "Configurations", "service-account.json"));

            Application.Run(new BooksForm());
        }
    }
}
