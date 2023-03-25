﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Database.Models
{
    public class AccountDTO
    {
        /// <summary>
        /// Unique id of account.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// SHA-256 hash of password.
        /// </summary>
        public string AccountPassword { get; set; }

        /// <summary>
        /// Collection of account ids representing friends.
        /// </summary>
        public AccountRelationDTO[] Friends { get; set; }

        /// <summary>
        /// Collection of account ids representing ignored.
        /// </summary>
        public AccountRelationDTO[] Ignored { get; set; }

        /// <summary>
        /// Collection of ladder stats.
        /// </summary>
        public int[] AccountWideStats { get; set; }

        /// <summary>
        /// Collection of custom ladder stats.
        /// </summary>
        public int[] AccountCustomWideStats { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? ClanId { get; set; }
        
        /// <summary>
        /// Application specific user data as a Base64 encoded string.
        /// Convert back to byte array for use with Medius application.
        /// </summary>
        public string MediusStats { get; set; }

        /// <summary>
        /// Machine id tied to account.
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// Whether or not the player has been banned.
        /// </summary>
        public bool IsBanned { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? AppId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Metadata { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ResetPasswordOnNextLogin { get; set; }
    }

    public class CreateAccountDTO
    {
        /// <summary>
        /// Name of account.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// SHA-256 hash of password.
        /// </summary>
        public string AccountPassword { get; set; }

        /// <summary>
        /// DnasPostSignature result as Base64 encoded string.
        /// </summary>
        public string MachineId { get; set; }

        /// <summary>
        /// Application specific data encoded as a Base64 string.
        /// </summary>
        public string MediusStats { get; set; }

        /// <summary>
        /// Application id of the client.
        /// </summary>
        public int AppId { get; set; }
    }

    public class AccountRelationDTO
    {
        /// <summary>
        /// Unique id of account.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Account name.
        /// </summary>
        public string AccountName { get; set; }
    }

    public class AccountStatusDTO
    {
        /// <summary>
        /// Unique id of account.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// App id of the account.
        /// </summary>
        public int AppId { get; set; }

        /// <summary>
        /// Whether or not the user is logged in.
        /// </summary>
        public bool LoggedIn { get; set; }

        /// <summary>
        /// If set, which game the user is in.
        /// </summary>
        public int? GameId { get; set; }

        /// <summary>
        /// Name of game
        /// </summary>
        public string GameName { get; set; }

        /// <summary>
        /// If set, which channel the user is in.
        /// </summary>
        public int? ChannelId { get; set; }

        /// <summary>
        /// If set, which world the user is in.
        /// </summary>
        public int? WorldId { get; set; }
    }

    public class AccountPasswordRequest
    {
        public int AccountId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

}
