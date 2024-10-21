using System.Collections.ObjectModel;
using System.Diagnostics;
using Nalai.Helpers;
using Nalai.Models;
using Nalai.Services;
using Nalai.ViewModels.Windows;
using Nalai.Views.Windows;

namespace Nalai.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        const string DOWNLOAD_URL =
            "https://mirrors.tuna.tsinghua.edu.cn/github-release/VSCodium/vscodium/1.94.2.24286/VSCodium-win32-x64-1.94.2.24286.zip";

        [RelayCommand]
        private async Task OnNewTask()
        {
            var fileName = GetUrlInfo.GetFileName(DOWNLOAD_URL);

            var task = await NalaiDownService.NewTask(DOWNLOAD_URL, fileName, Environment.CurrentDirectory);

            var vm = new DownloadingWindowViewModel();
            var window = new DownloadingWindow(vm, DOWNLOAD_URL, task);
            window.Show();
        }
        
        [ObservableProperty]
        private ObservableCollection<Person> _basicListViewItems = GeneratePersons();

        private static ObservableCollection<Person> GeneratePersons()
        {
            var random = new Random();
            var persons = new ObservableCollection<Person>();

            var names = new[]
            {
                "John",
                "Winston",
                "Adrianna",
                "Spencer",
                "Phoebe",
                "Lucas",
                "Carl",
                "Marissa",
                "Brandon",
                "Antoine",
                "Arielle",
                "Arielle",
                "Jamie",
                "Alexzander"
            };
            var surnames = new[]
            {
                "Doe",
                "Tapia",
                "Cisneros",
                "Lynch",
                "Munoz",
                "Marsh",
                "Hudson",
                "Bartlett",
                "Gregory",
                "Banks",
                "Hood",
                "Fry",
                "Carroll"
            };
            var companies = new[]
            {
                "Pineapple Inc.",
                "Macrosoft Redmond",
                "Amazing Basics Ltd",
                "Megabyte Computers Inc",
                "Roude Mics",
                "XD Projekt Red S.A.",
                "Lepo.co"
            };

            for (int i = 0; i < 50; i++)
            {
                persons.Add(
                    new Person(
                        names[random.Next(0, names.Length)],
                        surnames[random.Next(0, surnames.Length)],
                        companies[random.Next(0, companies.Length)]
                    )
                );
            }

            return persons;
        }
    }
}