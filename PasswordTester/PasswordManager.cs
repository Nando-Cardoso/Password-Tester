/*
 * Program:         PasswordManager.exe
 * Module:          PasswordManager.cs
 * Date:            06/02/2020
 * Author:          Fernando Rodrigues Cardoso
 * Description:     Password Manager application. which replicates the project from INFO3138 Declarative Languages course.
 *                  This program reads and stores accounts objects in a json file using json schema to validate the data.
 *                  Many options like change account password, delete entries froma account list are available.
 */

using System;
using System.Collections.Generic;
using System.IO;            // File class
using Newtonsoft.Json;              // JsonConvert class
using Newtonsoft.Json.Schema;       // JSchema class
using Newtonsoft.Json.Linq;         // JObject class
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    class Program
    {

        // Declare constants for the file path names 
        private const string ACCOUNTS_FILE = "accounts_data.json";
        private const string SCHEMA_FILE = "accounts_schema.json";

        static void Main(string[] args)
        {

            bool done = false;
            List<Account> accounts_list = new List<Account>();

            //Read acocunts_schema.json file
            string json_schema;
            if (ReadFile(SCHEMA_FILE, out json_schema))
            {
                //Read accounts in accounts_data.json file
                string accountsData;
                if (ReadFile(ACCOUNTS_FILE, out accountsData))
                {
                    try
                    {
                        accounts_list = JsonConvert.DeserializeObject<List<Account>>(accountsData);
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("\nERROR: Can not read the JSON File\n");
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("\nERROR: Can not convert the JSON data to a Library object.\n");
                    }

                }

                do
                {
                    PrintHeader(accounts_list);

                    //Bool variables to check for menu options pressed
                    bool validEntryMainMenu = false, subMenuPressed = false;

                    //Account index
                    int accountListIndex;

                    do
                    {

                        //If any option from the account submenu is pressed, get back to the main menu
                        if (subMenuPressed)
                        {
                            validEntryMainMenu = true;
                            continue;
                        }

                        //Check menu input
                        string ch = Console.ReadLine().ToLower();

                        //Add another entry
                        if (ch == "a")
                        {
                            Account newAccount;
                            bool isValid;
                            do
                            {
                                newAccount = new Account();
                                validEntryMainMenu = true;

                                Console.WriteLine("\nPlease key-in values for the following fields: \n");

                                Console.Write("Description: ");
                                newAccount.Description = Console.ReadLine();

                                Console.Write("User Id: ");
                                newAccount.UserId = Console.ReadLine();

                                Password newUserPassword = new Password();

                                Console.Write("Password: ");
                                newUserPassword.Value = Console.ReadLine();
                                PasswordTester pwTester = new PasswordTester(newUserPassword.Value);
                                pwTester._SetStrength(pwTester.Value);
                                newUserPassword.StrengthNum = pwTester.StrengthPercent;
                                newUserPassword.StrengthText = pwTester.StrengthLabel;
                                newUserPassword.LastReset = DateTime.Now.ToShortDateString();
                                newAccount.Password = newUserPassword;

                                Console.Write("Login url: ");
                                newAccount.LoginUrl = Console.ReadLine();

                                Console.Write("Account #: ");
                                newAccount.AccountNum = Console.ReadLine();

                                //Validate account object with schema
                                IList<string> messages;
                                isValid = ValidateItem(newAccount, json_schema, out messages);
                                if (!isValid)
                                {
                                    Console.WriteLine("\nError: Invalid account information entered. Please try again.");
                                    // Report validation error messages
                                    foreach (string msg in messages)
                                        Console.WriteLine($"{msg}");
                                }
                            } while (!isValid);



                            accounts_list.Add(newAccount);
                        }

                        else if (ch == "x")
                        {
                            validEntryMainMenu = true;
                            done = true;
                            continue;
                        }
                        else if (int.TryParse(ch, out accountListIndex))
                        {
                            bool validEntryAccountMenu = false;
                            try
                            {
                                PrintAccountInfo(accounts_list[accountListIndex - 1], accountListIndex);
                            }

                            //Exception if index number written does not exist in account list
                            catch (Exception ex)
                            {
                                Console.Write("\nAccount index does not exist! Please try another command: ");
                                continue;
                            }

                            //Printing account sub menu
                            Console.WriteLine("+---------------------------------------------------------------+");
                            Console.WriteLine("|     Press P to change this password                           |");
                            Console.WriteLine("|     Press D to delete this entry.                             |");
                            Console.WriteLine("|     Press M to return to the main menu.                       |");
                            Console.WriteLine("+-------------------------------------------------------------- +");

                            Console.Write("\nEnter a command: ");

                            do
                            {
                                string ch2 = Console.ReadLine().ToLower();

                                //Password change entry
                                if (ch2 == "p")
                                {
                                    validEntryAccountMenu = true;
                                    subMenuPressed = true;
                                    Console.Write("\nEnter New Password: ");
                                    accounts_list[accountListIndex - 1].Password.Value = Console.ReadLine();
                                    PasswordTester pwTester = new PasswordTester(accounts_list[accountListIndex - 1].Password.Value);
                                    pwTester._SetStrength(pwTester.Value);
                                    accounts_list[accountListIndex - 1].Password.StrengthNum = pwTester.StrengthPercent;
                                    accounts_list[accountListIndex - 1].Password.StrengthText = pwTester.StrengthLabel;
                                    accounts_list[accountListIndex - 1].Password.LastReset = DateTime.Now.ToShortDateString();
                                    Console.WriteLine("\nPassword changed!\n");
                                    continue;
                                }

                                //Delete Entry
                                else if (ch2 == "d")
                                {
                                    validEntryAccountMenu = true;
                                    subMenuPressed = true;
                                    accounts_list.RemoveAt(accountListIndex - 1);
                                    Console.WriteLine("Account Deleted!\n");
                                }

                                //Back to Main Menu
                                else if (ch2 == "m")
                                {
                                    validEntryAccountMenu = true;
                                    subMenuPressed = true;
                                    Console.WriteLine("\n*** Back To Main Menu ***\n");
                                    continue;
                                }

                                //Command in account submenu was not valid
                                else
                                {
                                    Console.Write("\nCommand entered is not valid! Please try again: ");
                                }
                            } while (!validEntryAccountMenu);
                        }

                        //Command in main menu was not valid
                        else
                        {
                            Console.Write("\nCommand entered is not valid! Please try again: ");
                        }

                    } while (!validEntryMainMenu);
                } while (!done);

                // Write the Account list to a file
                string json_all = JsonConvert.SerializeObject(accounts_list);
                try
                {
                    File.WriteAllText(ACCOUNTS_FILE, json_all);
                    Console.WriteLine($"\n\nYour shopping list has been written to {ACCOUNTS_FILE}.\n");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"\n\nERROR: {ex.Message}.\n");
                }

            }
            else
            {
                Console.WriteLine("\nERROR: JSON Schema could not be found!\n");
            }

        }// END MAIN

        // Validates an item object against a schema (incomplete)
        private static bool ValidateItem(Account acc, string json_schema, out IList<string> messages)
        {
            // Convert item object to a JSON string 
            string json_data = JsonConvert.SerializeObject(acc);

            // Validate the data string against the schema contained in the 
            // json_schema parameter. Also, modify or replace the following 
            // return statement to return 'true' if item is valid, or 'false' 
            // if invalid.
            JSchema schema = JSchema.Parse(json_schema);
            JObject itemObj = JObject.Parse(json_data);
            return itemObj.IsValid(schema, out messages);

        } // end ValidateItem()

        //Prints Header with all accounts
        private static void PrintHeader(List<Account> account_list)
        {
            Console.WriteLine("\n+---------------------------------------------------------------+");
            Console.WriteLine("|                        Account Entries                        |");
            Console.WriteLine("+---------------------------------------------------------------+");
            int i = 1;
            foreach (Account acc in account_list)
            {
                Console.WriteLine(String.Format("|  {0, 0}. {1, -58}|", i, acc.Description));
                i++;
            }
            Console.WriteLine("+---------------------------------------------------------------+");
            Console.WriteLine("|     Press # from the above list to select an entry            |");
            Console.WriteLine("|     Press A to add a new entry                                |");
            Console.WriteLine("|     Press X to quit and save the accounts file.               |");
            Console.WriteLine("+---------------------------------------------------------------+");
            Console.Write("\n\nEnter a command: ");
        }

        //Prints specific information about an Account
        private static void PrintAccountInfo(Account acc, int i)
        {
            Console.WriteLine("+---------------------------------------------------------------+");
            Console.WriteLine(String.Format("| {0, 0}. {1, -59}|", i, acc.Description));
            Console.WriteLine("+---------------------------------------------------------------+");
            Console.WriteLine(String.Format("| {0,-18} {1, -43}|", "UserId:", acc.UserId));
            Console.WriteLine(String.Format("| {0,-18} {1, -43}|", "Password:", acc.Password.Value));
            Console.WriteLine(String.Format("| {0,-18} {1, -43}|", "Password Strength:", acc.Password.StrengthText + " (" + acc.Password.StrengthNum + "%)")); ;
            Console.WriteLine(String.Format("| {0,-18} {1, -43}|", "Password Reset:", acc.Password.LastReset));
            Console.WriteLine(String.Format("| {0,-18} {1, -43}|", "Login url:", acc.LoginUrl));
            Console.WriteLine(String.Format("| {0,-18} {1, -43}|", "Account #:", acc.AccountNum));
        }

        // Attempts to read the json file specified by 'path' into the string 'json'
        // Returns 'true' if successful or 'false' if it fails
        private static bool ReadFile(string path, out string json)
        {
            try
            {
                // Read JSON file data 
                json = File.ReadAllText(path);
                return true;
            }
            catch
            {
                json = null;
                return false;
            }
        }

    } // end class
}
