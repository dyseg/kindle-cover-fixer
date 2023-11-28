﻿using System.Data.SQLite;
using System.Data;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Kindle_Cover_Fixer_V2
{
    // this file have the functions about the book finder in your library
    public partial class MainWindow
    {
        private void FindBooksTask()
        {
            string library = string.Empty;
            IsOnKindlePreparation();
            Dispatcher.Invoke(() =>
            {
                transferButton.IsEnabled = false;
                generateButton.IsEnabled = false;
                transferButton.IsEnabled = false;
                InitDataGridFindBooks();
                DataGridSystem.Items.Clear();
                library = libraryPath.Text;
                resultLabel.Content = string.Empty;
                runningNow.Content = Strings.Finding;
                
            });
            File.Copy(library + @"\metadata.db", UsefulVariables.GetKindleCoverFixerPath() + @"\metadata.db", true);
            string cs = @"URI=file:" + UsefulVariables.GetKindleCoverFixerPath() + @"\metadata.db";
            using SQLiteConnection connection = new(cs);
            connection.Open();
            using (SQLiteCommand selectCMD = connection.CreateCommand())
            {
                selectCMD.CommandText = "SELECT COUNT(*) FROM books";
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = 0;
                    progressBar.Maximum = int.Parse(selectCMD.ExecuteScalar().ToString()!);
                });              
                selectCMD.CommandText = "SELECT * FROM books";
                selectCMD.CommandType = CommandType.Text;
                SQLiteDataReader myReader = selectCMD.ExecuteReader();
                int bookCounter = 1;
                int errorCounter = 0;
                while (myReader.Read())
                {
                    string InKindle = string.Empty;
                    string problems = string.Empty;
                    string bookPath = library + @"\" + myReader["path"].ToString();
                    string uuid = myReader["uuid"].ToString()!;
                    string bookUuid = RealBookUuid(bookPath);
                    string bookTitle = myReader["title"].ToString()!;
                    bool checkError = false;
                    if (IsOnKindle(uuid, bookCounter))
                    { 
                        InKindle = Strings.Yes;                     
                    }
                    else
                    {
                        checkError = true;
                        InKindle = Strings.No;
                    }
                    if (!File.Exists(bookPath + @"\cover.jpg"))
                    {
                        checkError = true;
                        problems = Strings.NoCover;

                    }
                    else
                    {
                        problems = Strings.NoProblem;
                    }
                    if (!Regex.IsMatch(uuid, "[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}", RegexOptions.IgnoreCase))
                    {
                        checkError = true;
                        problems = Strings.NoUuid;
                    }
                    else
                    {
                        problems = Strings.NoProblem;
                    }
                    if (checkError)
                    {                    
                        errorCounter++;
                    }
                    if (bookUuid.Length != 36)
                    {
                        checkError = true;
                        problems = Strings.NoUuid;
                    }
                    LogLine("LIST", uuid +  " | " + bookTitle! + " | Transferable: " + InKindle + " | Errors: " + problems);
                    string generateIt = Strings.No;
                    Dispatcher.Invoke(() =>
                    {   
                        if (!checkError)
                        {
                            generateIt = Strings.Yes;
                            DataGridSystem.Items.Add(new DataGridSystemCols { FileNumber = bookCounter, FileName = bookTitle!, FilePath = bookPath, FileUuid = bookUuid, FileCan = generateIt });
                        }                      
                        DataGridUser.Items.Add(new DataGridFindBooks { FileNumber = bookCounter.ToString(), FileName = bookTitle!, FileUuid = bookUuid, FileInKindle = InKindle, FileProblems = problems});
                        DataGridUser.ScrollIntoView(DataGridUser.Items.GetItemAt(DataGridUser.Items.Count - 1));
                    });              
                    bookCounter++;
                    Thread resize = new(ResizeThread);
                    resize.Start();
                    Dispatcher.Invoke(() =>
                    {
                        progressBar.Value++;
                    });
                }
                connection.Close();
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value++;
                    if (errorCounter > 0)
                    {
                        resultLabel.Content = Strings.FinishListingWithErrors + errorCounter + Strings.Of + DataGridUser.Items.Count + Strings.HaveError;
                        LogLine("ERROR", "Book listing process finished with errors in: " + errorCounter + " of " + DataGridUser.Items.Count + "Books" );
                    }
                    else
                    {
                        resultLabel.Content = Strings.FinishListing;
                        LogLine("SUCCESS", "Book listing process finished.");
                    }                
                });             
            }
            
            Thread resizeEnd = new(ResizeThread);
            resizeEnd.Start();
            Dispatcher.Invoke(() => 
            {
                runningNow.Content = Strings.BookListed;
                EnableControl(generateButton);
            });            
        }
    }
}
