# Nalai

### [English](./README.md) | 简体中文
 
一个现代且快速的下载器，支持异步多线程分块下载。

前端为C#编写，使用 [WPF UI](https://github.com/lepoco/wpfui) 实现 Fluent Design

后端使用Rust编写，目前性能还可以（核心在这里： [nalai_core](https://github.com/sout233/nalai_core)）。

经测试，**Nalai**目前在Windows10以及Windows11上工作良好。

> [!IMPORTANT]
> 目前该项目正处于快速开发阶段，部分功能可能尚未实现。

## Roadmap

- [x] 新建下载任务
- [x] 多线程下载
- [x] 下载列队列表
- [ ] 浏览器事件传递
- [ ] 更好的错误处理
- [x] 重定向获取
- [ ] 大文件冷缓存
- [ ] 内存缓冲区占用优化
- [x] 性能优化
