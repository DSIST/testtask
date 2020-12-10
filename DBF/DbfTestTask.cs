using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DbfTests
{
    [TestClass]
    public class DbfTestTask
    {
        [TestMethod]
        public void TestTask()
        {
            const string RootDir = @".\Data";
            const string RelevantFileName = "128.dbf";

            var reader = new DbfReader();

            string[] files = Directory.GetFiles(RootDir, RelevantFileName, SearchOption.AllDirectories);
            OutputRow.Headers.AddRange(files);

            var outputs = new List<OutputRow>();
            List<(DateTime, string, double?)> dataValues = new List<(DateTime, string, double?)>();
            foreach (string file in files)
            {
                dataValues.AddRange(reader.ReadValues(file).Select(x => (x.Timestamp, file, (double?)x.Value)));
            }

            foreach (var group in dataValues.GroupBy(x => x.Item1).OrderBy(x => x.Key))
            {
                outputs.Add(new OutputRow
                {
                    Timestamp = group.Key,
                    Values = group.ToList().Select(x => x.Item3).ToList()
                });
            }

            // the following asserts should pass
            Assert.AreEqual(25790, outputs.Count);
            Assert.AreEqual(27, OutputRow.Headers.Count);
            Assert.AreEqual(27, outputs[0].Values.Count);
            Assert.AreEqual(27, outputs[11110].Values.Count);
            Assert.AreEqual(27, outputs[25789].Values.Count);
            Assert.AreEqual(633036852000000000, outputs.Min(o => o.Timestamp).Ticks);
            Assert.AreEqual(634756887000000000, outputs.Max(o => o.Timestamp).Ticks);
            Assert.AreEqual(633036852000000000, outputs[0].Timestamp.Ticks);
            Assert.AreEqual(634756887000000000, outputs.Last().Timestamp.Ticks);

            // write into file that we can compare results later on (you don't have to do something)
            string content = "Time\t" + string.Join("\t", OutputRow.Headers) + Environment.NewLine +
                          string.Join(Environment.NewLine, outputs.Select(o => o.AsTextLine()));
            File.WriteAllText(@".\output.txt", content);
        }
    }
}
