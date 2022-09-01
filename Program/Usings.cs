global using static System.Environment;

global using static ScheduleBot.Program;
global using static ScheduleBot.ScheduleFinder;
global using static ScheduleBot.Notifier;
global using static ScheduleBot.EnvironmentExtension;

global using File = System.IO.File;

global using ScheduleBot.Enums;
global using ScheduleBot.Structs;
global using ScheduleBot.DataBase;

global using Telegram.Bot;
global using Telegram.Bot.Exceptions;
global using Telegram.Bot.Polling;
global using Telegram.Bot.Types;
global using Telegram.Bot.Types.Enums;
global using Telegram.Bot.Types.InputFiles;

global using CloudConvert.API;
global using CloudConvert.API.Models.ExportOperations;
global using CloudConvert.API.Models.ImportOperations;
global using CloudConvert.API.Models.JobModels;
global using CloudConvert.API.Models.TaskOperations;

global using Microsoft.EntityFrameworkCore;

global using NLog;