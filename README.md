# 鸣谢

该项目基于angturil的 [SongRequestManager](https://github.com/angturil/SongRequestManager).  
© 2018 angturil

# 点歌管理器V2 SongRequestManagerV2

使用聊天核心[Chat Core](https://github.com/baoziii/ChatCore-v2)的点歌管理器 **使用其他版本可能会出现不兼容**


# Mod介绍

歌曲请求管理器Song Request Manager是一款可集成到节奏光剑BeatSaber的完全可自定义的点歌机器人或控制台。它最初是为了重写[EnhancedTwitchChat](https://github.com/brian91292/BeatSaber-EnhancedTwitchChat)中内置的点歌机器人，但是其功能范围升级飞快。现在它是一个独立模块。该模块需要使用的增强直播聊天[EnhancedStreamChat](https://github.com/brian91292/EnhancedStreamChat)以及聊天核心[StreamCore](https://github.com/brian91292/StreamCore)(后升级为[ChatCore](https://github.com/brian91292/ChatCore))是原作者对增强Twitch聊天[EnhancedTwitchChat](https://github.com/brian91292/BeatSaber-EnhancedTwitchChat)升级而来。

该mod的[原版](https://github.com/angturil/SongRequestManager)由angturil维护，[V2版](https://github.com/denpadokei/SongRequestManagerV2)由denpadokei维护

# TTS备注

如果你在使用TTS，如果你想减少机器人产生的垃圾消息。你可以通过几种方法解决。

第一种是从TTS中过滤机器人的用户名。

第二种是在你的TTS客户端中过滤含有感叹号的行

```
在RequestBotSettings.ini文件中
BotPrefix="! "
```

# 特性

具有60多个命令且功能不断增强的全功能点歌机器人。
完全可自定义的命令，每个命令都可以具有多个别名，权限和自定义帮助文本。
可使用"屏蔽列表"，“重映射”，“评分过滤器”，“谱师列表”等高级过滤器。
直接在直播中显示您的点歌队列和状态。
根据用户级别指定不同的请求限制。
房管可以使用全套命令来管理队列。
一个游戏内控制台，允许玩家无需搜索或下载就可以播放请求的歌曲。
功能齐全的twitch键盘，可以在twitch上进行聊天交互！
直接从控制台直接搜索歌曲，而不必退出到歌曲浏览器或下载器。
在Beatsaver.com上挑选并播放40首最新发布的歌曲。
配置文件备份。

更多功能正在测试中，即将发布！

# 依赖项

`BS Utils`, `SiraUtil`, `BSML`以及[`ChatCore`](https://github.com/baoziii/ChatCore-v2).

# 安装

安装所有的依赖项并且将文件解压到游戏根目录

# 用法

在主菜单的右上角会显示一个点歌按钮。如果队列里有歌曲的话会变绿，但是你可以不管它。别忘了在你准备好以后`开启队列`。在你`关闭队列`之前它会一直监听消息。`开启队列`按钮在点歌面板的右下角。

# 配置文件

配置文件保存在`UserData\Song Request ManagerV2 RequestBotSettings.ini`。*所有的设置项在保存的时候都是即时生效的！这意味着你不需要重启游戏就能看到更改！* 使用下面的表格来帮助你设置值(**注意：**你需要设置好频道才能开始监听点歌请求)

# 直播登录信息 

所有的设置项直接从ChatCore获取。所以无需在配置文件中更改。

# 编译

自行编译该mod只需要clone仓库并且更新项目references到`Beat Saber\Beat Saber_Data\Managed` 和`Beat Saber\Plugins` 文件夹编译即可。如果你的Beat Saber目录和编译后事件中设置的不一样，那么你可能需要更改以下目录里地址。

# 提示

该插件完全免费。如果你想打赏作者，可以到[贝宝Paypal](https://paypal.me/sehria)打赏。

# 下载  

[点此下载最新版](https://github.com/baoziii/SongRequestManagerV2/releases/latest)



# Credit  

This program is based on angturil's [SongRequestManager](https://github.com/angturil/SongRequestManager).  
© 2018 angturil

# SongRequestManagerV2
ChatCoreに対応したリクエストマネージャー  

（2020/6/24追記）そろそろ開発者さんが正式に対応しそうなので開発打ち止め。


# Mod Info
~~You need to StreamCore and ChatCore  
https://github.com/brian91292/StreamCore~~  
[ChatCore](https://github.com/brian91292/ChatCore)  
[SiraUtil](https://github.com/Auros/SiraUtil)  
~~Song Request Manager is an integrated, fully Customizable song request bot and Console for BeatSaber. It started life as an extensive rewrite of the built in song request bot in https://github.com/brian91292/BeatSaber-EnhancedTwitchChat, but quickly grew in scope and features. Its now a separate but dependent module. This mod and its companions, EnhancedStreamChat and StreamCore, are direct upgrades from the original EnhancedTwitchChat release, by the original authors.~~

This mod has been in use for many months, and is constantly updated. While this is a first official release as a standalone mod, its been in continuous use for months now, and is extensively tested. The bot is compatible with release 0.13.1. 

# Current State

The current bot is compatible with Release 1.0.0. Use the latest installer.

# TTS Notes
If you're using TTS, you'll want to reduce the amount of spam the bot produces. You can do this a number of ways. Filtering out your Name from TTS, or 
```
in RequestBotSettings.ini
BotPrefix="! "
```
then filter out the ! lines on your tts client.

# Features
```
  Full featured request bot with over 60 commands, and growing.
  Completely customizable, Every commmand can have multiple aliases, permissions and custom help text.
  Advanced filtering with Banlists, remapping, rating filters, mapper lists, and more.
  Display your song request queue and status directly on the stream.
  Different request limits based on the users subscription level.
  A full set of moderator commands to manage the queue.
  An ingame console allowing the player to play the requested songs without having to search or downnload.
  A full featured twitch keyboard allowing interaction with twitch chat!
  Direct search of song directly from the console, without ever having to exit to song browser or downloader.
  Pick and play any of the latest 40 posted songs off Beatsaver.com.
  Configuration Backup.
  
  Many more features are being tested and will be released soon!.
```

# Dependencies
~~Enhanced Twitch Chat depends on [EnhancedStreamChat] and [StreamCore], [CustomUI](https://www.modsaber.org/mod/customui/), [SongLoader](https://www.modsaber.org/mod/song-loader/), and [AsyncTwitch](https://www.modsaber.org/mod/asynctwitchlib/). Make sure to install them, or Song Request Manager Chat won't work!~~  
[CustomUI](https://www.modsaber.org/mod/customui/), [SongLoader](https://www.modsaber.org/mod/song-loader/), and [ChatCore](https://github.com/brian91292/ChatCore).

# Installation
Copy SongRequestManagerV2.dll to your Beat Saber\Plugins folder, and install all of its dependencies. That's it!

# Usage
A song request icon will appear on the upper right of the main menu. It will be green if there are song requests in the queue, but you can press it regardless. Don't forget to Open the queue for requests when you are ready. It will stay that way until you close it again. The Open Queue button is on the lower right of the song request panel.

# Setup
Needs more documentation

# Config
The configuration files are located under ~~UserData\EnhancedTwitchChat~~  UserData\Song Request ManagerV2 RequestBotSettings.ini ~~and TwitchLoginInfo.ini~~ are the two files you need to adjust. *Keep in mind all config options will update in realtime when you save the file! This means you don't have to restart the game to see your changes!* Use the table below as a guide for setting these values (**NOTE:** You will need to setup your channel info to be able to receive song requests.)

~~# TwitchLoginInfo.ini~~
~~| Option | Description |
| - | - |
| **TwitchChannelName** | The name of the Twitch channel whos chat you want to join (this is your Twitch username if you want to join your own channel) |
| **TwitchUsername** | Your twitch username for the account you want to send messages as in chat (only matters if you're using the request bot) |
| **TwitchOAuthToken** | The oauth token corresponding to the TwitchUsername entered above ([Click here to generate an oauth token](https://twitchapps.com/tmi/))  |~~  

# StreamLoginInfo  
All settings are automatically retrieved from ChatCore. There is no need to tamper with your login information in this configuration file.

# RequestBotSettings.ini
| Option | Description |
| ------------------------------- | ------------------------------------------------------------ |
| **PersistentRequestQueue=True** | Resets the queue at the start of session - this will soon change to reset the queue after session reset, like the duplicate and played lists. |
| **RequestHistoryLimit=100** | How many entries are key in the history list of songs that you've already played/skipped |
| **RequestBotEnabled** | When set to true, users can make song requests in chat.      |
| **UserRequestLimit=2** | Number of simulataneous song requests in the queue per tier|
| **SubRequestLimit=5** ||
| **ModRequestLimit=10** ||
| **VipBonusRequests=1** | VIP's are treated as a bonus over their regular level. A non subbed VIP would get 3 song requests.|
| **SessionResetAfterXHours=6** | Amount of time after session ENDS before your Duplicate song list and Played list are reset.|
| **LowestAllowedRating=40** | Lowest allowed rating (as voted on [BeatSaver.com](on https://Beatsaver.com)) permitted. Unrated songs get a pass.|
| **AutopickFirstSong=False** | If on, will simply pick the first song. Otherwise, the recommended method shows a list of possible songs that match your search. Careful use of Block and Remap will make this method more effective over time |
| **UpdateQueueStatusFiles=True** | Enables the generation of queuestatus.txt and queuelist.txt. Use StreamOBS' Text (GDI+) option to display your queue status and list on your live stream! |
| **MaximumQueueTextEntries=8** | How many entries are sent to the queuelist.txt file. Any entries beyond that will display a ... |
| **BotPrefix =""** | This adds a prefix to all bot output, set it to "! " to allow filtering of all bot output by TTS or Enhanced Twitch chat. You can use other means like filtering by name to achiveve this |
| **MixerUserName** | It works with or without.　|


# Compiling
To compile this mod simply clone the repo and update the project references to reference the corresponding assemblies in the `Beat Saber\Beat Saber_Data\Managed` and `Beat Saber\Plugins` folder, then compile. You may need to change the post build event if your Beat Saber directory isn't at the same location as mine.

# Tips
This plugin is free. If you wish to help us out though, tips to 
[our Paypal](https://paypal.me/sehria) are always appreciated.

# Download  
[Click here to download the latest SongRequestManager.dll](https://github.com/denpadokei/SongRequestManagerV2/releases/latest)