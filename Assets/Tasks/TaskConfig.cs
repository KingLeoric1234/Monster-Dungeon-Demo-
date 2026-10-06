namespace Game.Tasks
{
    /// <summary>任务配置：所有任务数据在这里定义。</summary>
    public static class TaskConfig
    {
        public const string TalkToNpcId = "task_talk_npc";
        public const string KillSlimeId = "task_kill_slime";
        public const string CollectKeyId = "task_collect_key";
        public const string KillBomberId = "task_kill_bomber";
        public const string FindTreasureId = "task_find_treasure";
        public const string RescueVillagerId = "task_rescue_villager";
        public const string DefeatBossId = "task_defeat_boss";
        public const string CollectHerbId = "task_collect_herb";

        public static Task CreateTalkToNpc() => new Task
        {
            id = TalkToNpcId, type = TaskType.TalkToNPC, status = TaskStatus.Active,
            taskName = "Talk to the NPC",
            targetText = "Reach the dungeon entrance",
            description = "The old blacksmith at the entrance has something to say. Go talk to him and learn about the dangers ahead.",
            giverName = "Blacksmith",
            reward = "100 Gold, Iron Sword"
        };

        public static Task CreateKillSlime() => new Task
        {
            id = KillSlimeId, type = TaskType.KillEnemy, status = TaskStatus.Active,
            taskName = "Slime Cleanup",
            targetText = "Kill 5 Slimes (0/5)",
            description = "Slimes have been invading the eastern fields and scaring travelers. Clear them out so people can pass safely. They're weak but they reproduce fast, so don't let them multiply.",
            giverName = "Guard Captain",
            reward = "200 Gold, Leather Armor"
        };

        public static Task CreateCollectKey() => new Task
        {
            id = CollectKeyId, type = TaskType.PickupItem, status = TaskStatus.Active,
            taskName = "Find the Lost Key",
            targetText = "Recover the storage key",
            description = "The merchant lost his storage key somewhere in the first dungeon. It should be near the old torch on the wall. Find it and bring it back.",
            giverName = "Traveling Merchant",
            reward = "150 Gold, Health Potion x3"
        };

        public static Task CreateKillBomber() => new Task
        {
            id = KillBomberId, type = TaskType.KillEnemy, status = TaskStatus.Active,
            taskName = "Bomber Hunt",
            targetText = "Kill 3 Bombers (0/3)",
            description = "Bombers are unstable and dangerous. They wander deep in the dungeon and explode when you get close. Take them out before they hurt anyone. Ranged weapons recommended.",
            giverName = "Guard Captain",
            reward = "300 Gold, Bomb Bag"
        };

        public static Task CreateFindTreasure() => new Task
        {
            id = FindTreasureId, type = TaskType.PickupItem, status = TaskStatus.Active,
            taskName = "Hidden Treasure",
            targetText = "Find the hidden chest",
            description = "Legend says an old adventurer buried a chest near the second dungeon. Look around the broken pillars and follow the stone path. Nobody has come back with proof yet.",
            giverName = "Wandering Scholar",
            reward = "500 Gold, Ancient Map"
        };

        public static Task CreateRescueVillager() => new Task
        {
            id = RescueVillagerId, type = TaskType.TalkToNPC, status = TaskStatus.Active,
            taskName = "Missing Villager",
            targetText = "Rescue the trapped villager",
            description = "A young villager got lost exploring the dungeon. Search the second floor carefully. He might be hiding behind a crate, too scared to move. Bring him back alive.",
            giverName = "Village Elder",
            reward = "400 Gold, Silver Ring"
        };

        public static Task CreateDefeatBoss() => new Task
        {
            id = DefeatBossId, type = TaskType.KillEnemy, status = TaskStatus.Active,
            taskName = "Slay the Dungeon Lord",
            targetText = "Defeat the Boss (0/1)",
            description = "The Dungeon Lord has taken over the deepest chamber. He grows stronger with every fallen adventurer. This is the final trial. Bring your best equipment and don't die.",
            giverName = "Village Elder",
            reward = "1000 Gold, Legendary Weapon"
        };

        public static Task CreateCollectHerb() => new Task
        {
            id = CollectHerbId, type = TaskType.PickupItem, status = TaskStatus.Active,
            taskName = "Herb Gathering",
            targetText = "Collect 10 Herbs (0/10)",
            description = "The old healer's hands shake as she hands you a tattered list. Three leaves of moonwort, two roots of ironbark, and a single drop of silver moss. She says the village child has been burning for three days, and every hour that passes without the medicine is an hour closer to the grave. The herbs grow in the damp cracks of the first dungeon wall, but the dark is full of things that crawl. Take a torch. Move quietly. Bring them back before the dew dries, or the medicine will spoil. She also warned you not to eat the silver moss. It glows, she said, because it remembers the dead. Do not follow the glow deeper into the dark. The light you are looking for is small, close, and never reaches for you. If it reaches back, run.",
            giverName = "Village Healer",
            reward = "250 Gold, Healing Potion x5"
        };
    }
}
