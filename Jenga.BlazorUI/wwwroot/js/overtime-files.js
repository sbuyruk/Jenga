window.overtimeFiles = {
    async download(name, contentType, stream) {
        const buffer = await stream.arrayBuffer();
        const url = URL.createObjectURL(new Blob([buffer], { type: contentType }));
        const link = document.createElement('a');
        link.href = url;
        link.download = name;
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
};
