function Background() {
    return (
        <div aria-hidden className="pointer-events-none absolute inset-0 overflow-hidden bg-surface">
            <div className="absolute inset-0
                bg-[radial-gradient(ellipse_75%_55%_at_70%_100%,rgba(130,196,178,0.55)_0%,rgba(130,196,178,0.25)_45%,rgba(130,196,178,0)_100%)]" />
            <div className="absolute inset-0 
            bg-[radial-gradient(ellipse_80%_60%_at_0%_100%,rgba(43,108,176,0.55)_0%,rgba(43,108,176,0.28)_40%,rgba(43,108,176,0)_100%)]" />
        </div>
    );
}

export default Background;