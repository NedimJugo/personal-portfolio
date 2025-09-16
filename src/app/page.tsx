export default function HomePage() {
  return (
    <div className="min-h-screen">
      <main className="container mx-auto px-4 py-16">
        <div className="text-center">
          <h1 className="text-4xl md:text-6xl font-bold mb-6">
            Hi, I'm <span className="text-primary">Your Name</span>
          </h1>
          <p className="text-xl text-muted-foreground mb-8 max-w-2xl mx-auto">
            Full Stack Developer passionate about creating amazing web experiences
          </p>
          <div className="flex gap-4 justify-center">
            <button className="bg-primary text-primary-foreground px-6 py-3 rounded-lg hover:bg-primary/90">
              View My Work
            </button>
            <button className="border border-primary text-primary px-6 py-3 rounded-lg hover:bg-primary/10">
              Contact Me
            </button>
          </div>
        </div>
      </main>
    </div>
  )
}