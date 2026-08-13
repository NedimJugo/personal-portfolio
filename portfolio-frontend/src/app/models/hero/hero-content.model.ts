// models/hero/hero-content.model.ts
export interface HeroContent {
  badgeText: string;
  title: string;
  subtitle: string;
  description: string;
  primaryButtonText: string;
  secondaryButtonText: string;
  characterImageUrl: string;
  speechBubbleText: string;
  flipPhotoUrl: string;
  flipPhotoCaption: string;
}

export const DEFAULT_HERO_CONTENT: HeroContent = {
  badgeText: "⚡ Available for Hire!",
  title: "Hey! I'm <span class=\"highlight-text\">Nedim Jugo</span>",
  subtitle: "Full-Stack Developer & Digital Craftsman",
  description: "I turn coffee into code and ideas into reality. Specializing in building modern web applications that users actually enjoy using!",
  primaryButtonText: "View My Work",
  secondaryButtonText: "Get In Touch",
  characterImageUrl: "https://ecochallengeblob.blob.core.windows.net/ecochallenge/ChatGPT%20Image%20Oct%201,%202025,%2012_13_18%20AM.png",
  speechBubbleText: "Let's build something amazing!",
  flipPhotoUrl: 'https://ecochallengeblob.blob.core.windows.net/ecochallenge/134308049.jpeg',
  flipPhotoCaption: 'Discover more about me!'

};