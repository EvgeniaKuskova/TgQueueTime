namespace TgQueueTime;

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
    WaitingForNumberWindowToAccept
}