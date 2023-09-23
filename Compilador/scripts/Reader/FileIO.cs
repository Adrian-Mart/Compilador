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

        /// <summary>
        /// Initializes a new instance of the <see cref="FileIO"/> class.
        /// </summary>
        /// <param name="fileExtension">The file extension of the output file.</param>
        /// <param name="processorPath">Path to the processor data.</param>
        private protected FileIO(string fileExtension, string processorPath)
        {
            this.fileExtension = fileExtension;
            processor = GetProcessorFromFile(processorPath);
        }

        /// <summary>
        /// Reads the content of a file from the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>The content of the file as a string.</returns>
        private protected string ReadFileContent(string filePath)
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
                    if (line.StartsWith('#')) continue;
                    sb.Append(line);
                    sb.Append('\n');
                }
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Writes the content of a file to the specified file path.
        /// </summary>
        /// <param name="filePath">The path of the file to write.</param>
        public virtual void WriteFileContent(string filePath)
        {
            string text = ReadFileContent(filePath);
            string processedInput = processor.GetOutputString(text);
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
    }

}