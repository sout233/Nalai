chrome.webRequest.onCompleted.addListener(
    function(details) {
        console.log("Start Listening")
        // 检查是否为下载请求
        if (details.tabId > 0 && details.type === "main_frame") {
            // 发送POST请求到WPF程序
            fetch('http://localhost:10721/', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ url: details.url }) // 将下载的URL作为参数发送
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => console.log(data))
                .catch(error => console.error('Error sending POST request:', error));
        }
    },
    {urls: ["<all_urls>"]}, // 监听所有URL
    ["responseHeaders"] // 需要响应头以检查是否为下载
);