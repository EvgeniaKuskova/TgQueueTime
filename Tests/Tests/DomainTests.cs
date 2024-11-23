using Xunit;

namespace Domain;

/*public class DynamicQueueTests
{
    [Fact]
    public void AddClient_ToEmptyQueue_SetsServiceStartTimeCorrectly()
    {
        // Arrange
        var service = new Service("Test Service", TimeSpan.FromMinutes(10));
        var organization = new Organization(1, "Test Org",
            new List<Service> { new Service("Test Service", TimeSpan.FromMinutes(10)) }, 2);
        var beginTime = TimeSpan.FromMinutes(5);

        var dynamicQueue = new DynamicQueue(new List<Service>(){service}, organization, 1);

        var client = new Client(1, service);
        var beforeAdd = DateTime.Now;
        
        dynamicQueue.AddClient(client);
        var afterAdd = DateTime.Now;

        // Assert
        Assert.Single(dynamicQueue.Queue);
        var queueItem = dynamicQueue.Queue.First();
        var expectedServiceStartTimeLowerBound = beforeAdd.Add(beginTime);
        var expectedServiceStartTimeUpperBound = afterAdd.Add(beginTime);
        Assert.True(queueItem.ServiceStartTime >= expectedServiceStartTimeLowerBound &&
                    queueItem.ServiceStartTime <= expectedServiceStartTimeUpperBound,
            $"ServiceStartTime {queueItem.ServiceStartTime} не находится в ожидаемом диапазоне от {expectedServiceStartTimeLowerBound} до {expectedServiceStartTimeUpperBound}");
    }


    [Fact]
    public void AddMultipleClients_ServiceStartTimesAreCalculatedCorrectly()
    {
        // Arrange
        var service1 = new Service("Test Service1", TimeSpan.FromMinutes(10));
        var service2 = new Service("Test Service2", TimeSpan.FromMinutes(12));
        var organization = new Organization(1, "Test Org",
            new List<Service> { new Service("TestServies", TimeSpan.FromMinutes(5)) }, 2);
        var beginTime = TimeSpan.FromMinutes(5);

        var dynamicQueue = new DynamicQueue(new List<Service>(){service1, service2}, organization, 2);

        var client1 = new Client(1, service1);
        var client2 = new Client(2, service1);
        var client3 = new Client(3, service2);

        var baseTime = DateTime.Now;

        // Act
        dynamicQueue.AddClient(client1); // Expected ServiceStartTime = baseTime + 5 minutes
        dynamicQueue.AddClient(client2); // Expected ServiceStartTime = (baseTime + 5) + 10 = baseTime + 15 minutes
        dynamicQueue.AddClient(client3); // Expected ServiceStartTime = (baseTime + 15) + 10 = baseTime + 25 minutes

        // Assert
        Assert.Equal(3, dynamicQueue.Queue.Count);

        var queueItem1 = dynamicQueue.Queue[0];
        var queueItem2 = dynamicQueue.Queue[1];
        var queueItem3 = dynamicQueue.Queue[2];

        Assert.Equal(baseTime.Add(beginTime).ToString(), queueItem1.ServiceStartTime.ToString());
        Assert.Equal(queueItem1.ServiceStartTime.Add(service1.AverageTime).ToString(),
            queueItem2.ServiceStartTime.ToString());
        Assert.Equal(queueItem2.ServiceStartTime.Add(service2.AverageTime).ToString(),
            queueItem3.ServiceStartTime.ToString());
    }


    /*[Fact]
    public void DeleteClient_RecalculatesServiceStartTimesCorrectly()
    {
        // Arrange
        var service = new Service("Test Service", TimeSpan.FromMinutes(10));
        var organization = new Organization(3L,"Test Org",
            new List<Service> { new Service("TestServies", TimeSpan.FromMinutes(5)) },  2);
        var beginTime = TimeSpan.FromMinutes(5);
        var endTime = TimeSpan.FromHours(8);

        var dynamicQueue = new DynamicQueue(service, organization, beginTime, endTime);

        var client1 = new Client(1);
        var client2 = new Client(2);
        var client3 = new Client(3);

        var baseTime = DateTime.Now;

        // Act
        dynamicQueue.AddClient(client1); // baseTime + 5 minutes
        dynamicQueue.AddClient(client2); // baseTime + 15 minutes
        dynamicQueue.AddClient(client3); // baseTime + 25 minutes

        // Удаляем первого клиента
        dynamicQueue.DeleteClient(client1);

        // Assert
        Assert.Equal(2, dynamicQueue.Queue.Count);

        var queueItem2 = dynamicQueue.Queue[0];
        var queueItem3 = dynamicQueue.Queue[1];

        // После удаления, ServiceStartTime для client2 должен быть базовое время + 5 минут
        Assert.Equal(baseTime.Add(beginTime).ToString(), queueItem2.ServiceStartTime.ToString());
        // ServiceStartTime для client3 должен быть baseTime + 5 + 10 = baseTime + 15 минут
        Assert.Equal(queueItem2.ServiceStartTime.Add(service.AverageTime).ToString(),
            queueItem3.ServiceStartTime.ToString());
    }

    [Fact]
    public void AddClient_ServiceExceedsEndTime_ClientNotAdded()
    {
        // Arrange
        var service = new Service("Test Service", TimeSpan.FromMinutes(10));
        var organization = new Organization(1, "Test Org",
            new List<Service> { new Service("TestServies", TimeSpan.FromMinutes(5)) }, 2);
        var beginTime = TimeSpan.FromMinutes(5);
        var endTime = TimeSpan.FromMinutes(15); // Рабочее время заканчивается через 15 минут

        var dynamicQueue = new DynamicQueue(service, organization, beginTime, endTime);

        var client1 = new Client(1);
        var client2 = new Client(2);

        var baseTime = DateTime.Now;

        // Act
        dynamicQueue.AddClient(client1); // Запланировано на baseTime + 5 минут
        dynamicQueue.AddClient(client2); // Запланировано на baseTime + 15 минут

        // Попытка добавить третьего клиента, который должен начать обслуживание в baseTime + 25 минут, что выходит за пределы рабочего времени
        var client3 = new Client(3);
        dynamicQueue.AddClient(client3);

        // Assert
        Assert.Equal(2, dynamicQueue.Queue.Count); // Третий клиент не должен быть добавлен
        Assert.Contains(dynamicQueue.Queue, q => q.Client.Id == 1);
        Assert.Contains(dynamicQueue.Queue, q => q.Client.Id == 2);
        Assert.DoesNotContain(dynamicQueue.Queue, q => q.Client.Id == 3);
    }
}*/