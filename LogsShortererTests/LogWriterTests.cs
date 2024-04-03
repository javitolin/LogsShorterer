using LogsShorterer.Entities;
using LogsShorterer.Writer;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace LogsShorterer.Tests
{
    [TestClass()]
    public class LogWriterTests
    {
        [TestMethod]
        public void LogWriter_LargeRequiredKey_ReturnsCantBePrinted()
        {
            // Arrange
            Mock<IWriter> writerMock = new Mock<IWriter>();
            LogWriter logWriter = new(writerMock.Object, new[] { "Level", "Message" });

            LogEntity logEntity = new LogEntity
            {
                Message = string.Join("", Enumerable.Repeat("LargeMessage", 100))
            };

            // Act
            var result = logWriter.PrintLogEntity(logEntity, 10);

            // Assert
            Assert.AreEqual(SplitResult.CANT_BE_PRINTED, result);
            writerMock.Verify(wm => wm.Write(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void LogWriter_LargeMultipleRequiredKey_ReturnsCantBePrinted()
        {
            // Arrange
            Mock<IWriter> writerMock = new Mock<IWriter>();
            LogWriter logWriter = new(writerMock.Object, new[] { "Level", "Message" });

            LogEntity logEntity = new LogEntity
            {
                Level = LogLevel.Error,
                Message = "Small"
            };

            // Act
            var result = logWriter.PrintLogEntity(logEntity, 10);

            // Assert
            Assert.AreEqual(SplitResult.CANT_BE_PRINTED, result);
            writerMock.Verify(wm => wm.Write(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void LogWriter_SmallLogMessage_ReturnsShorterThanMax()
        {
            // Arrange
            Mock<IWriter> writerMock = new Mock<IWriter>();
            LogWriter logWriter = new(writerMock.Object, new[] { "Level", "Message" });

            LogEntity logEntity = new LogEntity
            {
                Level = LogLevel.Error,
                Message = "Small"
            };

            // Act
            var result = logWriter.PrintLogEntity(logEntity, 1000);

            // Assert
            Assert.AreEqual(SplitResult.SHORTER_THAN_MAX, result);
            writerMock.Verify(wm => wm.Write(It.Is<string>(str => JsonConvert.DeserializeObject(str) != null)), Times.Once());
        }

        [TestMethod]
        public void LogWriter_SplittedLogMessage_ReturnsSplitted()
        {
            // Arrange
            Mock<IWriter> writerMock = new Mock<IWriter>();
            LogWriter logWriter = new(writerMock.Object, new[] { "Level", "Message" });

            LogEntity logEntity = new LogEntity
            {
                Level = LogLevel.Error,
                Message = "Small"
            };

            // Act
            var result = logWriter.PrintLogEntity(logEntity, 100);

            // Assert
            Assert.AreEqual(SplitResult.SPLITTED, result);
            writerMock.Verify(wm => wm.Write(It.Is<string>(str => JsonConvert.DeserializeObject(str) != null)), Times.Exactly(3));
        }
    }
}