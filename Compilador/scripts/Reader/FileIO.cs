using System;
using System.IO;
using System.Text;
using Compilador.Processors;

namespace Compilador.IO
{
    /// <summary>
    /// Abstract class for reading and writing files with a specified file extension.
    /// </summary>
    public abstract class FileIO
    {
        /// <summary>
        /// The file extension of the output file.
        /// </summary>
        private protected string fileExtension = ".txt";

        /// <summary>
        /// The processor in charge of processing the input.
        /// </summary>
        private protected IProcessor processor;

        public IProcessor Processor { get => processor; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIO"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension of the output file.</param>
        /// <param name="processorPath">Path to the processor data.</param>
        private protected FileIO(string fileExtension, string processorPath, string saveToFilePath)
        {
            this.fileExtension = fileExtension;
            processor = GetProcessorFromFile(processorPath, saveToFilePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIO"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension of the output file.</param>
        /// <param name="processorPath">Path to the serialized processor data.</param>
        private protected FileIO(string fileExtension, string serialDataPath)
        {
            this.fileExtension = fileExtension;
            processor = GetProcessorFromSerialFile(serialDataPath) ?? throw new Exception("Invalid serial data.");
        }

        /// <summary>
        /// Reads the content of a file from the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>The content of the file as a string.</returns>
        public string ReadFileContent(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    sb.Append(line);
                    sb.Append('\n');
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reads the content of a processor file from the specified file path.
        /// Ignores all the lines that start with a '#'.
        /// </summary>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>The content of the file as a string.</returns>
        private protected string ReadProcessorFileContent(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith('#')
                        || string.IsNullOrEmpty(line)
                        || string.IsNullOrWhiteSpace(line))
                        continue;
                    sb.Append(line);
                    sb.Append('\n');
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Writes the output of the processor to the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the file to write.</param>
        public virtual void WriteFileContent(object input, string filePath)
        {
            string processedInput = processor.GetOutputString(input);
            using (StreamWriter writer = new StreamWriter(filePath + fileExtension))
            {
                writer.Write(processedInput);
            }
        }

        /// <summary>
        /// Gets the processor from the specified file path.
        /// </summary>
        /// <param name="processorPath">Path to the processor data.</param>
        /// <returns> Processor obj from the specified file</returns>
        private protected abstract IProcessor GetProcessorFromFile(string processorPath);

        /// <summary>
        /// Gets the processor from the specified file path and saves it to the specified file path.
        /// </summary>
        /// <param name="processorPath">Path to the processor data.</param>
        /// <param name="saveToFilePath">Path to the file where the
        /// processor will be saved.</param>
        /// <returns> Processor obj from the specified file</returns>
        private protected abstract IProcessor GetProcessorFromFile(string processorPath, string saveToFilePath);

        /// <summary>
        /// Gets the processor from the specified serialized data file path.
        /// </summary>
        /// <param name="processorPath">Path to the serialized processor data.</param>
        /// <returns> Processor obj from the specified file</returns>
        private protected abstract IProcessor? GetProcessorFromSerialFile(string processorPath);

        /// <summary>
        /// Gets the output of the processor from the specified input.
        /// </summary>
        /// <param name="input">The input to be processed. For Lexer,
        /// this is a string, for Parser, this is a TokenStream.</param>
        /// <returns>The output of the processor. For Lexer, this is a TokenStream,
        /// for Parser, this is a SyntaxTree.</returns>
        public virtual object GetOutput(object input)
        {
            return processor.GetOutputObject(input);
        }
    }

}