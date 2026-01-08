// <copyright file="OnLoginUserEventArgs.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using SkyDrop.Models;

namespace SkyDrop.Events;

public class OnLoginUserEventArgs : EventArgs
{
    public OnLoginUserEventArgs(LoginUser? loginUser)
    {
        this.LoginUser = loginUser;
    }

    public LoginUser? LoginUser { get; }
}