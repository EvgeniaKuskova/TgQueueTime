namespace TelegramBots;

public enum UserState
{
    Start,
    WaitingForNameOrganization,
    WaitingForNameService,
    WaitingForAverageTime,
    WaitingForNumbersWindow,
    WaitingForAverageTimeUpdate,
    WaitingForNameServiceUpdate,
    WaitingForNumberWindowGet,
    WaitingForNumberWindowToAccept,

    ClientStart,
    WaitingClientForGetNameOrganization,
    WaitingClientForNameService,
    WaitingClientForMyTime,
    WaitingClientForCountClientsBefore,
    WaitingClientForNameOrganization,
    WaitingNotification
}