# AbyssGet
C# abyss.to Parallel CLI downloader

# Usage
```
Usage:
  AbyssGet <videoIds>... [options]

Arguments:
  <videoIds>  Video ID `K8R6OOjS7` or Player URL `https://abysscdn.com/?v=K8R6OOjS7`

Options:
  -f, --first-url-only                               Only use the first CDN url [default: False]
  -ps, --best-url-pool-size <best-url-pool-size>     Size of the URL pool containing the best performing URLs [default: 3]
  -t, --max-threads <max-threads>                    Maximum number of threads [default: 16]
  -l, --log-level <Debug|Error|Information|Warning>  Log level [default: Information]
  -rt, --request-timeout <request-timeout>           Request timeout in seconds [default: 120]
  -bt, --block-timeout <block-timeout>               Block timeout in seconds [default: 60]
  -rr, --request-retries <request-retries>           Number of request retries [default: 3]
  -p, --download-in-parallel                         Download videos in parallel [default: False]
  -s, --save-dir <save-dir>                         Directory to save downloaded files [default: .]
  -n, --save-name <save-name>                       Custom name for the downloaded file (without extension) [default: ""]
  -a, --auto-select                                 Automatically select the best quality to download [default: False]
  --version                                          Show version information
  -?, -h, --help                                     Show help and usage information
```