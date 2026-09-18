# Romodoro para Avalonia — Especificação de Design e Arquitetura

## Objetivo

Implementar os três frames do Figma “Tempo — Relógio, Cronômetro e Pomodoro” como uma aplicação desktop Avalonia funcional para Windows, macOS e Linux. O Figma é a fonte de verdade para composição, dimensões, cores, tipografia e estados visuais iniciais.

## Referências do Figma

| Modo | Nó | Tamanho-base |
| --- | --- | --- |
| Relógio | `2:8` | 330 × 410 |
| Cronômetro | `2:64` | 340 × 410 |
| Pomodoro | `2:32` | 400 × 660 |

Os frames compartilham uma janela arredondada, escura e sem decoração nativa visível. A navegação superior ocupa uma superfície secundária e contém três modos com ícone, rótulo e indicador coral para o item ativo.

## Sistema visual

### Cores

- Fundo da janela: `#131416`.
- Superfície de navegação: `#1B1C1F`.
- Borda e divisores: `#383B40`.
- Texto principal: `#F0F0F2`.
- Texto secundário: `#9699A3`.
- Destaque e estado ativo: `#FF6661`.

### Tipografia

A família Inter será incorporada como recurso do aplicativo para consistência entre sistemas. O relógio e os contadores usam peso Medium entre 48 e 56 px; rótulos usam Regular ou SemiBold entre 12 e 15 px. A renderização respeitará escala de DPI sem substituir a família por fontes específicas do sistema.

### Forma e espaçamento

- Cartão externo com raio de 24 px, borda de 1 px e sombra ampla.
- Navegação interna com margem de 15 px, altura de 78 px e raio de 18 px.
- Ícones principais de 24 px.
- Botões de ação circulares de 54 px; ação primária de 58 px.
- Rodapé separado por divisor a 23 px das laterais, contendo alternador e rótulo “Sempre no topo”.

## Arquitetura

A solução terá um projeto Avalonia Desktop e um projeto de testes. A aplicação seguirá MVVM leve com interfaces próprias, `INotifyPropertyChanged` e comandos sem dependência de toolkit.

### Camadas

- `Views`: janela principal e controles visuais reutilizáveis.
- `ViewModels`: estado apresentado, comandos e coordenação dos modos.
- `Models`: estados e regras dos modos, sem dependência da interface.
- `Services`: relógio monotônico, agendamento de atualizações, notificações, som e integração com a janela.

O tempo transcorrido será calculado a partir de um relógio monotônico, não pela contagem de ticks, evitando deriva quando a thread de UI atrasar.

## Composição da interface

### Janela principal

A janela será transparente, sem moldura e arrastável pela área não interativa. Um cartão interno desenhará fundo, borda, raio e sombra. Controles discretos no canto superior direito permitirão minimizar e fechar.

Ao alternar modos, a janela animará para o tamanho-base correspondente. `MinWidth` e `MinHeight` preservarão a composição; o conteúdo escalará com DPI. Caso o gerenciador de janelas não permita animação ou transparência completa, o estado final continuará correto sem depender desses efeitos.

### Navegação de modos

Um controle reutilizável apresentará Relógio, Cronômetro e Pomodoro. O modo ativo usa `#FF6661`, peso SemiBold e linha inferior. Modos inativos usam `#9699A3`. Cada item aceita clique, foco e ativação por teclado.

### Rodapé

O alternador “Sempre no topo” atualiza imediatamente a propriedade `Topmost`. O estado é compartilhado pelos modos e permanece ativo durante a execução do aplicativo.

## Comportamentos

### Relógio

- Atualiza hora e data uma vez por segundo.
- Usa o fuso e calendário locais do sistema.
- Exibe hora em formato de 24 horas e data em português, conforme o frame.
- A atualização é retomada corretamente após suspensão do computador.

### Cronômetro

Estados: parado, em execução e pausado.

- Iniciar começa ou retoma a medição.
- Pausar congela a apresentação sem perder o acumulado.
- Reiniciar retorna a zero e remove voltas.
- Marcar volta registra o instante acumulado atual.
- A lista de voltas aparece em região expansível abaixo dos controles, mantendo o frame inicial intacto quando vazia.
- A precisão apresentada é de segundos, coerente com o Figma; internamente a medição preserva precisão maior.

### Pomodoro

Etapas: foco de 25 minutos, pausa curta de 5 minutos e pausa longa de 15 minutos.

- O ciclo inicia em foco.
- Após cada foco, incrementa-se o total de sessões concluídas.
- Depois do primeiro, segundo e terceiro foco, inicia-se pausa curta.
- Depois do quarto foco, inicia-se pausa longa e o contador do ciclo é reiniciado após sua conclusão.
- O término de uma etapa dispara notificação e som e inicia automaticamente a próxima etapa.
- Iniciar, pausar, reiniciar e avançar etapa ficam disponíveis conforme o estado.
- O anel mostra a fração restante da etapa e é atualizado pelo tempo monotônico.
- Reiniciar volta ao início da etapa atual; não apaga focos concluídos no ciclo.

## Notificações e som

`INotificationService` e `ISoundService` isolam diferenças entre plataformas. A implementação escolhe o mecanismo suportado em tempo de execução. Se notificações nativas não estiverem disponíveis, a aplicação mostra uma indicação visual acessível e preserva o som; se o áudio falhar, a notificação visual continua funcionando. Falhas não interrompem a progressão do temporizador.

O som de conclusão será um recurso curto incorporado, livre de dependência de rede. Nenhuma permissão será solicitada antecipadamente; o sistema operacional poderá exibir seu próprio pedido quando necessário.

## Recursos visuais

Os SVGs exportados pelo Figma serão baixados e incorporados ao projeto, pois os URLs temporários expiram. Não serão redesenhados manualmente. Cada ativo terá largura e altura explícitas em seu contêiner Avalonia.

## Responsividade e multiplataforma

- Layouts usarão `Grid`, `StackPanel` e alinhamentos relativos; coordenadas absolutas ficarão restritas a detalhes que dependem da geometria do frame.
- A janela manterá as proporções e espaçamentos em escalas comuns de 100%, 125%, 150% e 200%.
- Texto longo ou diferenças métricas não poderão sobrepor controles.
- Operações de plataforma serão encapsuladas e possuirão fallback.
- Nenhuma API exclusiva do Windows será usada fora de uma implementação condicional de serviço.

## Acessibilidade

- Todos os botões terão nomes acessíveis, estados e dicas de uso.
- A ordem de tabulação seguirá navegação, conteúdo, ações e rodapé.
- Enter e Espaço ativarão controles focados.
- Indicadores ativos não dependerão somente de cor: peso tipográfico e sublinhado também comunicarão o estado.
- Atualizações de tempo frequentes não serão anunciadas continuamente por leitores de tela; conclusões de etapas serão anunciadas.

## Tratamento de erros

- Erros de notificação e áudio serão capturados, registrados e convertidos em fallback visual.
- Uma exceção de atualização não encerrará o aplicativo; o próximo pulso recalculará o estado a partir do relógio monotônico.
- Ativos ausentes serão detectados no build.

## Testes e validação

### Testes automatizados

- Formatação de hora e data.
- Início, pausa, retomada, reinício e voltas do cronômetro.
- Sequência completa do Pomodoro, incluindo pausa longa após quatro focos.
- Recalculo correto após atraso ou suspensão simulada.
- Comandos e mudanças de propriedades dos view models.
- Fallback quando notificação ou som falham.

### Verificação visual

- Capturar Relógio, Cronômetro e Pomodoro nos tamanhos-base.
- Comparar hierarquia, posições, cores, tipografia e alinhamentos com os frames.
- Verificar pelo menos duas escalas de DPI disponíveis no ambiente.
- Executar uma rodada de correções agrupadas e uma confirmação final.

### Verificação técnica

- Restaurar, compilar e executar os testes da solução.
- Publicar ou compilar para os runtimes desktop suportados quando os workloads locais permitirem.
- Garantir que nenhum ativo dependa dos URLs temporários do Figma.

## Limites desta entrega

Não haverá histórico persistente, configurações customizáveis de duração, sincronização em nuvem, telemetria ou execução em bandeja. Esses recursos não aparecem no Figma nem foram solicitados e podem ser adicionados posteriormente sem alterar o núcleo dos temporizadores.
