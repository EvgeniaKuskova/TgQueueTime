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
    WaitingClientForNameOrganization,
    WaitingClientForNameService,
    WaitingClientForMyTime,
    WaitingClientForCountClientsBefore,
}