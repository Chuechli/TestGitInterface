using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GitInterface {
    /// <summary>
    /// The Validator Utility class contains method to validate the user input in special cases (e.g. email).
    /// </summary>
    public static class ValidatorUtility {
        public static bool IsValidEmail(string email) {
            try {
                var testEmail = new System.Net.Mail.MailAddress(email.Replace(" ", ""));
                return testEmail.Address == email;
            } catch {
                return false;
            }
        }
    }
}
