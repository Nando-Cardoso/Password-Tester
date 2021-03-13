/*
 * Program:         PasswordManager.exe
 * Date:            May 20, 2019
 * Revision:        May 24, 2019 - Renamed this class from 'Password' to 'PasswordTester'
 * Course:          INFO-3138
 * Description:     Password strength analyzer loosely based on http://www.passwordmeter.com/
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManager
{
    class Account
    {
        public string Description { get; set; }
        public string UserId { get; set; }
        public string LoginUrl { get; set; }
        public string AccountNum { get; set; }
        public Password Password { get; set; }
    } // end class Player

    class Password
    {
        public string Value { get; set; }
        public int StrengthNum { get; set; }
        public string StrengthText { get; set; }
        public string LastReset { get; set; }
    }// end class Password
}
