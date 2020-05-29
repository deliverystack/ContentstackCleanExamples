using Contentstack.Core.Models;

namespace cslist
{
    using Contentstack.Core;
    using McMaster.Extensions.CommandLineUtils; // alternative to abandonware Microsoft.Extensions.CommandLineUtils.
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class Program
    {
        private class Startup
        {
            public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                Contentstack.AspNetCore.IServiceCollectionExtensions.AddContentstack(services, configuration);
            }
        }

        private static void ExitMessage(CommandLineApplication app,
            string message,
            bool showHelp = true,
            bool exit = false,
            ExitCode exitCode = ExitCode.Success,
            Exception ex = null)
        {
            if (!String.IsNullOrEmpty(message))
            {
                Console.WriteLine(Environment.NewLine + app.Name + " : " + message + Environment.NewLine);
            }
            else if (ex != null)
            {
                Console.WriteLine(ex.Message);
            }

            if (showHelp)
            {
                app.ShowHelp();
            }

            if (ex != null)
            {
                Console.WriteLine(Environment.NewLine + ex + " : " + ex.Message);
                Console.WriteLine(ex.StackTrace + Environment.NewLine);
            }

            if (exit)
            {
                Environment.Exit((int)exitCode);
            }
        }
        private static void Message(string message, bool addLine = false)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine($"{DateTime.Now:h:mm:ss.fffffff} : {typeof(Program)} : " + message);
            }

            if (addLine)
            {
                Line();
            }
        }

        private static void Line()
        {
            Message("========================================================================================");
        }

        private enum ExitCode
        {
            Success,
            Exception,
            HelpRequested,
            DirectoryDoesNotExist
        }

        static void Main(string[] args)
        {
            Message($"{typeof(Program)} : Main()", true);
            CommandLineApplication app = new CommandLineApplication();

            try
            {
                app.Description = ".NET Core console app to validate a Contentstack stack.";
                app.Name = typeof(Program).Namespace;
                CommandOption help = app.HelpOption("-?|-h|--help");
                CommandOption dir = app.Option("-d|--d<value>",
                    "Path to directory containing appconfig.json",
                    CommandOptionType.SingleValue);
                app.OnExecute(() =>
                {
                    if (help.HasValue())
                    {
                        ExitMessage(app, null, false, true, ExitCode.HelpRequested);
                    }

                    string directory = Directory.GetCurrentDirectory();

                    if (dir.HasValue())
                    {
                        directory = dir.Value();
                    }

                    if (!Directory.Exists(directory))
                    {
                        ExitMessage(app,
                            $"{directory} does not exist or is not a subdirectory.",
                            true,
                            true,
                            ExitCode.DirectoryDoesNotExist);
                    }

                    IConfiguration configuration = new ConfigurationBuilder().AddJsonFile(
                        $"{directory}\\appsettings.json",
                        optional: false,
                        reloadOnChange: true).Build();
                    Startup startup = new Startup();
                    ServiceCollection serviceCollection = new ServiceCollection(); // dependency injection configuration 
                    startup.ConfigureServices(serviceCollection, configuration); // configuration
                    IServiceProvider provider = serviceCollection.BuildServiceProvider(); // caution: may be disposable 
                    ContentstackClient client = provider.GetService<ContentstackClient>();
                    //                    client.SerializerSettings.Converters.Add(new MBFlexibleblocksFlexibleblocksConverter());
                    Console.WriteLine(Environment.NewLine);
                    Message(typeof(EntryLister) + ".List()", true);
                    new EntryLister().List(client);
                    Message(typeof(EntryLister) + ".List() complete.");
                    Console.WriteLine(Environment.NewLine);
                    // Message(typeof(SimpleEntryLister) + ".List()", true);
                    // new SimpleEntryLister(client).List();
                    // Message(typeof(SimpleEntryLister) + ".List() complete.");
                    Console.WriteLine(Environment.NewLine);
                    //                    Message(typeof(BlockLister) + ".List()", true);
                    //                    new BlockLister().List(
                    //                        client.ContentType("flexibleblockspage").Entry("blt36bfa917ffedd575").Fetch<Flexibleblockspage>().Result);
                    //                    Message(typeof(BlockLister) + ".List() complete.");
                });

                app.Execute(args);

            }
            catch (Exception ex)
            {
                ExitMessage(app, $"{ex} : {ex.Message}", false, true, ExitCode.Exception);
            }
        }

        private class EntryLister
        {
            public void List(ContentstackClient client)
            {
                foreach (Newtonsoft.Json.Linq.JObject contentType in
                    client.GetContentTypes(new Dictionary<string, object>()).Result)
                {
                    string contentTypeUid = contentType.GetValue("uid").ToString();
                    Message(contentTypeUid);

                    client.ContentType(contentTypeUid).Query().Find<Entry>().ContinueWith((t) =>
                    {
                        foreach (Entry entry in t.Result.Items)
                        {
                            string line = entry.Uid;

                            if (entry.Object.ContainsKey("url")
                                && entry.Object["url"] != null
                                && !String.IsNullOrEmpty(entry.Object["url"].ToString()))
                            {
                                line += " (" + entry.Object["url"] + ")";
                            }

                            line += " : " + entry.Title;
                            Message(line);
                        }

                        Line();
                    }).GetAwaiter().GetResult();
                }
            }
        }

        /*
        private class SimpleEntry
        {
            public string Url { get; set; }
            public string Uid { get; set; }
            public string Title { get; set; }
        }

        private class SimpleEntryLister
        {
            public void List(ContentstackClient client)
            {
                foreach (Newtonsoft.Json.Linq.JObject contentType in
                    client.GetContentTypes(new Dictionary<string, object>()).Result)
                {
                    string contentTypeUid = contentType.GetValue("uid").ToString();
                    Message(contentTypeUid);

                    client.ContentType(contentTypeUid).Query().Find<SimpleEntry>().ContinueWith((t) =>
                    {
                        foreach (SimpleEntry entry in t.Result.Items)
                        {
                            string line = entry.Uid;

                            if (!String.IsNullOrEmpty(entry.Url))
                            {
                                line += " (" + entry.Url + ")";
                            }

                            line += " : " + entry.Title;
                            Message(line);
                        }

                        Line();
                    }).GetAwaiter().GetResult();
                }
            }
        }

        private class BlockLister
        {
            public void List(Flexibleblockspage page)
            {
                foreach (MBFlexibleblocksFlexibleblocks block in page.Pageblocks.Flexibleblocks)
                {
                    switch (block.BlockType)
                    {
                        case MBFlexibleblocksFlexibleblocksEnum.Imageblock:
                            Message(block.BlockType + " : " + ((MBFlexibleblocksImage) block).Image.Url);
                            break;
                        case MBFlexibleblocksFlexibleblocksEnum.Richtextblock:
                            Message(block.BlockType + " : " + ((MBFlexibleblocksRichTextBlock) block).Richtext);
                            break;
                        case MBFlexibleblocksFlexibleblocksEnum.Markdownblock:
                            Message(block.BlockType + " : " + ((MBFlexibleblocksMarkdownBlock) block).Markdown);
                            break;
                        default:
                            //TODO: exception type
                            throw new ApplicationException("Unrecognized block type : " + block.BlockType);
                            break; // leave it here for the next guy
                    }
                }
            }
        }
        */
    }
}