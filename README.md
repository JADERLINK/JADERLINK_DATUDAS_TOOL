# **JADERLINK_DATUDAS_TOOL**

Extract and Repack DAT/UDAS/MAP files (RE4 Little Endian) [2007/PS2/UHD/PS4/NS]

**Info:**
<br>License: MIT Licence
<br>Language: C#
<br>Platform: Windows
<br>Dependency: Microsoft .NET Framework 4.8

## Translate from Portuguese Brazil:

Programa destinado à extração e reempacotamento dos arquivos DAT/UDAS/MAP do RE4 das versões Little Endian do jogo.
<br>Criei esse programa como uma alternativa aos outros programas que têm a mesma proposta.

**Update V.1.0.4**
<br> Removido o suporte para os arquivos DAS do RE4VR, para esses arquivos use essa tool: [RE4_VR_OG_DAS_TOOLS](https://github.com/JADERLINK/RE4_VR_OG_DAS_TOOLS)
<br> Agora, por padrão o programa só vai gerar o arquivo idxJ, mas ainda tem um bat para gerar o arquivo idx;
<br> Feito melhorias na qualidade do código do programa;
<br> Adicionado suporte ao Linux via mono e seu sistema de diretório;

**Update V.1.0.3**
<br>Unificado as duas tools em uma, adicionada compatibilidade aos arquivos "DAS" da versão de RE4VR. 
<br>Foram feitas melhorias gerais no código.

**Update V.1.0.2**
<br>Melhorias, e ao fazer repack o programa faz o alinhamento dos arquivos no arquivo dat/udas/map.

## EXTRACT

Destinado a extrair os arquivos dat/udas/map, o programa vai criar uma pasta com o nome do arquivo, que vai conter os arquivos extraídos (sem a existência de subpastas), e também vai gerar dois arquivos ".idxJ" (que é um formato próprio do programa) e ".idx" (que é igual à versão do "Son of Persia" e do "MarioKartN64").
<br>Para recompilar o arquivo, será usado somente um dos arquivos .idx*.

## REPACK

Destinado a reconstruir os arquivos dat/udas/map, ele aceita como entrada os arquivos ".idx" ou ".idxj", você precisa somente de 1 dos dois, veja as especificações dos arquivos mais abaixo.

## Arquivo .idxJ
Explicação do arquivo, vou usar como exemplo o arquivo "r100.udas" como referência. Ao extrair, vai gerar o arquivo "r100.idxJ" (e o arquivo "r100.idx" que será explicado mais abaixo) uma pasta com o nome "r100" que vai ter os arquivos extraídos nela (sem subpastas).

**Conteúdo do idxJ**
<br>Nota: O conteúdo com // é informativo e não existe no arquivo original.
<br>Nota2: para fazer comentários no arquivo, use o caractere dois pontos ":"
<br>Nota3: caracteres **# / \\ : !** São usados para comentários.

```
# github.com/JADERLINK/JADERLINK_DATUDAS_TOOL
# youtube.com/@JADERLINK
# JADERLINK DATUDAS TOOL By JADERLINK
//versão de identificação do programa de extração
TOOL_VERSION:V04
//FILE_FORMAT formato do arquivo a ser recompilado
// sendo os formados suportados: UDAS, DAT, MAP
FILE_FORMAT:UDAS
//UDAS_TOP, arquivo opcional, não existe para os arquivos DAT/MAP
// representa o header do udas, 
//caso o arquivo exista, as informações necessárias serão sobrepostas, 
//caso o arquivo não exista, o programa gerara um header próprio.
//para essa variável funcionar, você deve tirar o caracter ! do começo do nome.
!UDAS_TOP:r100\r100_TOP.HEX
// quantidade de arquivos que vão no Dat/Map,
//no caso de se tratar de um arquivo Udas,
//saiba que existe um arquivo dat dentro do Udas
DAT_AMOUNT:51
// listagem dos arquivos, para adicionar novos arquivos, basta seguir o padrão
DAT_000:r100\r100_000.CAM
DAT_001:r100\r100_001.SAT
DAT_002:r100\r100_002.LIT
DAT_003:r100\r100_003.LIT
*
* // conteúdo omitido
*
DAT_048:r100\r100_048.FCV
DAT_049:r100\r100_049.FCV
DAT_050:r100\r100_050.FCV
//UDAS_SOUNDFLAG: tag exclusiva do Udas,
// caso ela exista, significa que à um arquivo SND no final do arquivo Udas
// a ausência dela, não haverá um arquivo no final do arquivo Udas
UDAS_SOUNDFLAG:4
// UDAS_MIDDLE, arquivo opcional, exclusiva do Udas,
// representa os bytes entre o arquivos dat e snd do Udas,
// são dados ignorados pelo jogo
//para essa variável funcionar, você deve tirar o caracter ! do começo do nome.
!UDAS_MIDDLE:r100\r100_MIDDLE.HEX
// UDAS_END, local do arquivo que fica no final do Udas,
// seria o arquivo SND, 
//nota: é indiferente o formato do arquivo para o programa
UDAS_END:r100\r100_END.SND
// textos iniciados com : ou # são apenas comentários
```

# Arquivo .idx
Leia a explicação acima.
<br>A explicação desse arquivo é definida, por como é entendida pelo meu programa, e não como foi originalmente pensado por "Son of Persia" e "MarioKartN64".

**Conteúdo do idx**
<br>Nota: O conteúdo com // é informativo e não existe no arquivo original.
<br>Nota2: caracteres **# / \\ : !** São usados para comentários.

```
//FileCount, quantidade total de arquivo dat mais o arquivo final do udas
FileCount = 52
//SoundFlag, caso a tag NÃO exista sera gerado um arquivo .DAT
// caso a tag exista sera gerado um arquivo .UDAS
// sendo que se o valor for 4, o último arquivo da listagem abaixo
// sera o arquivo final (SND) do arquivo Udas
// e caso o valor for 0 ou -1, não tera um arquivo final no Udas
SoundFlag = 4
// listagem dos arquivos, para adicionar novos arquivos, basta seguir o padrão
File_0 = r100\r100_000.CAM
File_1 = r100\r100_001.SAT
File_2 = r100\r100_002.LIT
File_3 = r100\r100_003.LIT
*
* // conteúdo omitido
*
File_48 = r100\r100_048.FCV
File_49 = r100\r100_049.FCV
File_50 = r100\r100_050.FCV
// caso for "SoundFlag = 4" o último arquivo sera o arquivo SND do Udas
File_51 = r100\r100_END.SND
```

## Comparação aos outros Programas

* Criei o programa por causa de alguns problemas com a versão do "MarioKartN64":
* A versão dele cria duas pastas (a principal e uma subpasta), e a minha versão cria somente uma pasta.
* Ao reempacotar o arquivo Udas, não é considerado o arquivo .idx, mas é colocado todos os arquivos que estão na subpasta, e a minha versão segue o que está no arquivo .idx
* A minha versão NÃO cria backup, diferente da versão do "MarioKartN64".
* O arquivo que ele chamou de "DAS" é nomeado como SND, que nem a versão do "Son of Persia"
* Adicionado compatibilidade com os arquivos Dat e Map, basicamente dentro do Udas tem um arquivo Dat, e o arquivo Map é um arquivo Dat com outra extensão.

**At.te: JADERLINK**
<br>2025-07-20