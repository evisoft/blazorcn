namespace BlazorCN.Demo.Data;

public static class MockData
{
    public static readonly DashboardStat[] DashboardStats =
    [
        new("Total Revenue", "$45,231.89", "+20.1% from last month"),
        new("Subscriptions", "+2,350", "+180.1% from last month"),
        new("Sales", "+12,234", "+19% from last month"),
        new("Active Now", "+573", "+201 since last hour"),
    ];

    public static readonly RecentSale[] RecentSales =
    [
        new("Olivia Martin", "olivia.martin@email.com", "+$1,999.00"),
        new("Jackson Lee", "jackson.lee@email.com", "+$39.00"),
        new("Isabella Nguyen", "isabella.nguyen@email.com", "+$299.00"),
        new("William Kim", "will@email.com", "+$99.00"),
        new("Sofia Davis", "sofia.davis@email.com", "+$39.00"),
    ];

    public static readonly TaskItem[] Tasks =
    [
        new("TASK-8782", "You can't compress the program without quantifying the open-source SSD pixel!", "In Progress", "High"),
        new("TASK-7878", "Try to calculate the EXE feed, maybe it will index the multi-byte pixel!", "Backlog", "Medium"),
        new("TASK-7839", "We need to bypass the neural TCP card!", "Todo", "High"),
        new("TASK-5562", "The SAS interface is down, bypass the open-source pixel!", "Backlog", "Medium"),
        new("TASK-8686", "I'll parse the wireless SSL protocol, that should driver the API panel!", "Canceled", "Low"),
        new("TASK-1280", "Use the digital TLS panel, then you can transmit the haptic system!", "Done", "High"),
        new("TASK-7262", "The UTF8 application is down, parse the neural bandwidth!", "Done", "High"),
        new("TASK-1138", "Generating the driver won't do anything, we need to quantify the 1080p SMTP bandwidth!", "In Progress", "Medium"),
        new("TASK-7184", "We need to program the back-end THX pixel!", "Todo", "Low"),
        new("TASK-5160", "Calculating the bus won't do anything, we need to navigate the back-end JSON protocol!", "In Progress", "High"),
        new("TASK-5618", "Generating the driver won't do anything, we need to index the online SSL application!", "Done", "Medium"),
        new("TASK-6699", "I'll transmit the wireless JBOD capacitor, that should hard drive the SSD feed!", "Backlog", "Medium"),
        new("TASK-2858", "We need to override the online UDP bus!", "Backlog", "Low"),
        new("TASK-9864", "I'll reboot the 1080p FTP panel, that should bandwidth the UTF8 bus!", "Todo", "High"),
        new("TASK-8722", "Use the virtual HDD interface, then you can parse the bluetooth alarm!", "In Progress", "Low"),
        new("TASK-3320", "Parsing the feed won't do anything, we need to copy the bluetooth DRAM circuit!", "Todo", "Medium"),
        new("TASK-9602", "Compressing the interface won't do anything, we need to compress the online SDD card!", "Done", "High"),
        new("TASK-4453", "Try to override the ASCII application, maybe it will index the multi-byte bandwidth!", "Canceled", "Medium"),
        new("TASK-3881", "We need to index the mobile PCI bus!", "In Progress", "Low"),
        new("TASK-3473", "The SQL firewall is down, input the digital port!", "Todo", "High"),
    ];
}

public record DashboardStat(string Title, string Value, string Change);
public record RecentSale(string Name, string Email, string Amount);
public record TaskItem(string Id, string Title, string Status, string Priority);
