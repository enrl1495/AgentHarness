using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace AgentHarness.Core.Services;

/// <summary>
/// A middleware that handles DeepSeek-specific requirements (like reasoning_content)
/// and standard mapping errors to ensure high availability.
/// </summary>
public class CompatibilityMiddleware : DelegatingChatClient
{
    public CompatibilityMiddleware(IChatClient innerClient) : base(innerClient)
    {
    }

    public override async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Limpieza proactiva de herramientas para modelos incompatibles
        if (options != null && options.Tools != null && options.Tools.Count > 0)
        {
            var model = options.ModelId?.ToLower() ?? "";
            bool isPhi = model.Contains("phi");
            bool isR1 = model.Contains("r1");

            if (isPhi || isR1)
            {
                options.Tools = null;
            }
        }

        try
        {
            // Antes de enviar, verificamos si hay mensajes de asistente con "pensamiento" 
            // que necesitemos formatear para DeepSeek.
            var processedMessages = messages.Select(m =>
            {
                if (m.Role == ChatRole.Assistant && m.AdditionalProperties != null && m.AdditionalProperties.ContainsKey("reasoning_content"))
                {
                    var reasoning = m.AdditionalProperties["reasoning_content"]?.ToString();
                    if (!string.IsNullOrEmpty(reasoning))
                    {
                        var newContent = new List<AIContent>();
                        newContent.Add(new TextContent($"<thought>\n{reasoning}\n</thought>\n\n{m.Text}"));
                        return new ChatMessage(ChatRole.Assistant, newContent);
                    }
                }
                return m;
            }).ToList();

            return await base.CompleteAsync(processedMessages, options, cancellationToken);
        }
        catch (Exception ex) when (IsCompatibilityError(ex))
        {
            // Fallback: Reintento sin herramientas si el error parece ser de soporte de tools
            if (options != null && (ex.Message.Contains("tools") || ex.Message.Contains("400")))
            {
                options.Tools = null;
                return await base.CompleteAsync(messages, options, cancellationToken);
            }

            return new ChatCompletion(new ChatMessage(ChatRole.Assistant,
                $"[Error de Compatibilidad]: {ex.Message}. " +
                "Esto suele ocurrir por el 'Thinking Mode' o falta de soporte de herramientas. " +
                "He intentado un reintento automático sin herramientas."))
            {
                FinishReason = ChatFinishReason.Stop,
                CompletionId = "compatibility-fallback-" + Guid.NewGuid().ToString("N")
            };
        }
    }

    private bool IsCompatibilityError(Exception ex)
    {
        var msg = ex.Message ?? "";
        return msg.Contains("ChatFinishReason") ||
               msg.Contains("reasoning_content") ||
               msg.Contains("400") ||
               ex is ArgumentOutOfRangeException;
    }

    public override async IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> messages,
        ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<StreamingChatCompletionUpdate>? updates = null;
        string? errorMsg = null;

        try
        {
            updates = base.CompleteStreamingAsync(messages, options, cancellationToken);
        }
        catch (Exception ex) when (IsCompatibilityError(ex))
        {
            errorMsg = ex.Message;
        }

        if (errorMsg != null || updates == null)
        {
            yield return new StreamingChatCompletionUpdate { Contents = { new TextContent($"[Error Inicial DeepSeek: {errorMsg}]") } };
            yield break;
        }

        var enumerator = updates.GetAsyncEnumerator(cancellationToken);
        while (true)
        {
            StreamingChatCompletionUpdate? update = null;
            try
            {
                if (!await enumerator.MoveNextAsync()) break;
                update = enumerator.Current;
            }
            catch (Exception ex) when (IsCompatibilityError(ex))
            {
                errorMsg = ex.Message;
            }

            if (errorMsg != null)
            {
                yield return new StreamingChatCompletionUpdate { Contents = { new TextContent($"\n\n[Error de Metadata DeepSeek: {errorMsg}]") } };
                yield break;
            }

            if (update != null) yield return update;
        }
    }
}
