#!/usr/bin/env node
// Adds aria-label to icon-only ButtonCn (Size="ButtonSize.Icon*") that lack one.
// Label is derived from the first Lucide icon inside (semantic map + PascalCase
// fallback). Handles both live markup and mirrored @code verbatim strings
// (doubled ""). Dry-run by default; pass --write to apply.
import { readFile, writeFile } from "node:fs/promises";
import { glob } from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";
import { findTagEnd } from "./_tagscan.mjs";

const repoRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), "..");
const WRITE = process.argv.includes("--write");
const TARGET = path.join(repoRoot, "docs", "BlazorCN.Demo", "Pages");

const SEMANTIC = {
  X: "Close", Trash: "Delete", Trash2: "Delete", Pencil: "Edit", PencilLine: "Edit",
  Edit: "Edit", MoreHorizontal: "More options", MoreVertical: "More options",
  EllipsisVertical: "More options", Ellipsis: "More options",
  ChevronLeft: "Previous", ChevronRight: "Next", ChevronsLeft: "First page",
  ChevronsRight: "Last page", ArrowLeft: "Back", ArrowRight: "Forward",
  ChevronUp: "Scroll up", ChevronDown: "Scroll down",
  Search: "Search", Bell: "Notifications", BellRing: "Notifications",
  Settings: "Settings", Settings2: "Settings", Copy: "Copy", Check: "Confirm",
  Plus: "Add", Minus: "Remove", Menu: "Menu", Heart: "Like", Star: "Favorite",
  Bookmark: "Bookmark", Share: "Share", Share2: "Share", Download: "Download",
  Upload: "Upload", RefreshCw: "Refresh", RotateCcw: "Undo", RotateCw: "Redo",
  Play: "Play", Pause: "Pause", SkipBack: "Previous track", SkipForward: "Next track",
  Volume2: "Volume", VolumeX: "Mute", Mic: "Microphone", MicOff: "Mute microphone",
  Send: "Send", Paperclip: "Attach file", Filter: "Filter", Funnel: "Filter",
  Eye: "Show", EyeOff: "Hide", Lock: "Lock", Unlock: "Unlock",
  Calendar: "Calendar", Clock: "Time", Info: "Information", CircleHelp: "Help",
  HelpCircle: "Help", ExternalLink: "Open in new tab", Link: "Copy link",
  Maximize: "Maximize", Maximize2: "Maximize", Minimize: "Minimize", Minimize2: "Minimize",
  ZoomIn: "Zoom in", ZoomOut: "Zoom out", Printer: "Print", Save: "Save",
  Sun: "Light mode", Moon: "Dark mode", LogOut: "Log out", LogIn: "Log in",
  User: "Account", CircleUser: "Account", ShoppingCart: "Cart", ShoppingBag: "Cart",
  Github: "GitHub", Twitter: "Twitter", Facebook: "Facebook", Instagram: "Instagram",
  Linkedin: "LinkedIn", Youtube: "YouTube", Mail: "Email", Phone: "Call",
  MessageSquare: "Message", MessageCircle: "Message", ThumbsUp: "Like", ThumbsDown: "Dislike",
  Flag: "Report", Ban: "Block", Archive: "Archive", Inbox: "Inbox",
  Grid3x3: "Grid view", LayoutGrid: "Grid view", List: "List view", Rows3: "List view",
  PanelLeft: "Toggle sidebar", PanelRight: "Toggle panel", SlidersHorizontal: "Filters",
  CirclePlus: "Add", CircleMinus: "Remove", CircleX: "Clear", XCircle: "Clear",
  Sparkles: "AI suggestions", Wand2: "Generate", Zap: "Quick action",
  Globe: "Language", Languages: "Language", Palette: "Theme", Camera: "Camera",
  Image: "Image", Video: "Video", File: "File", FileText: "Document", Folder: "Folder",
  Home: "Home", House: "Home", Map: "Map", MapPin: "Location", Navigation: "Navigate",
  Code: "Code", Terminal: "Terminal", Bug: "Report bug", GitBranch: "Branches",
  Database: "Database", Server: "Server", Cloud: "Cloud", Wifi: "Wi-Fi",
  Battery: "Battery", Bluetooth: "Bluetooth", Cast: "Cast", Airplay: "AirPlay",
  Shuffle: "Shuffle", Repeat: "Repeat", Rewind: "Rewind", FastForward: "Fast forward",
  Scissors: "Cut", Clipboard: "Paste", ClipboardCopy: "Copy", FilePlus: "New file",
  FolderPlus: "New folder", UserPlus: "Add user", UserMinus: "Remove user",
  Users: "Members", Building: "Organization", Briefcase: "Work",
  Package: "Package", Truck: "Shipping", CreditCard: "Payment", Wallet: "Wallet",
  Gift: "Gift", Tag: "Tag", Ticket: "Ticket", Percent: "Discount",
  TrendingUp: "Trends", BarChart: "Chart", BarChart3: "Chart", PieChart: "Chart",
  LineChart: "Chart", Activity: "Activity", Target: "Goal", Award: "Award",
  Smile: "Emoji", Laugh: "Emoji", Frown: "Emoji", Meh: "Emoji",
  AlarmClock: "Alarm", Timer: "Timer", Hourglass: "Pending", CalendarDays: "Calendar",
  Pin: "Pin", PinOff: "Unpin", Move: "Move", GripVertical: "Drag", GripHorizontal: "Drag",
  RefreshCcw: "Refresh", Grid3X3: "Grid view", Grid2X2: "Grid view", Grid: "Grid view",
  ArrowUp: "Move up", ArrowDown: "Move down", ArrowUpDown: "Sort", ArrowUpRight: "Open",
  CircleCheck: "Confirm", SquarePen: "Edit", PenLine: "Edit",
  ListFilter: "Filter", Columns3: "Columns", Table2: "Table view", Kanban: "Board view",
  Volume1: "Volume", Volume: "Volume", PhoneCall: "Call", PhoneOff: "End call",
  VideoOff: "Stop video", ScreenShare: "Share screen", MonitorUp: "Share screen",
  Hand: "Raise hand", Captions: "Captions", PictureInPicture: "Picture in picture",
  PictureInPicture2: "Picture in picture", Fullscreen: "Fullscreen", Expand: "Expand",
  Shrink: "Collapse", ChevronsUpDown: "Expand", ChevronsDownUp: "Collapse",
  PlusCircle: "Add", MinusCircle: "Remove", AlertCircle: "Alert", CircleAlert: "Alert",
  OctagonX: "Error", TriangleAlert: "Warning", ShieldCheck: "Security", Shield: "Security",
  KeyRound: "Password", Key: "Password", Fingerprint: "Biometric login",
  QrCode: "QR code", ScanLine: "Scan", Nfc: "NFC", Rss: "RSS feed",
  Sunrise: "Sunrise", Sunset: "Sunset", CloudSun: "Weather", CloudRain: "Weather",
  Slack: "Slack", Chrome: "Chrome", Figma: "Figma", Dribbble: "Dribbble", Twitch: "Twitch",
  Apple: "Apple", Podcast: "Podcast", Music: "Music", Music2: "Music", Disc3: "Music",
  Headphones: "Audio", Speaker: "Speaker", Radio: "Radio", Tv: "TV", Gamepad2: "Games",
};

function pascalToWords(s) {
  const words = s.replace(/([a-z0-9])([A-Z])/g, "$1 $2").replace(/([A-Z]+)([A-Z][a-z])/g, "$1 $2").split(" ");
  const cleaned = words.filter(w => !/^\d+$/.test(w));
  const label = cleaned.join(" ").toLowerCase();
  return label.charAt(0).toUpperCase() + label.slice(1);
}
const labelFor = (icon) => SEMANTIC[icon] || pascalToWords(icon);

async function processFile(fp) {
  const src = await readFile(fp, "utf8");
  let out = "";
  let pos = 0;
  let changed = 0;
  const stats = [];
  for (;;) {
    const start = src.indexOf("<ButtonCn", pos);
    if (start === -1) break;
    const after = src[start + 9];
    if (after && !/[\s/>]/.test(after)) { out += src.slice(pos, start + 9); pos = start + 9; continue; }
    const openEnd = findTagEnd(src, start); // quote-aware: attr values may hold '>' (lambdas, [&>div])
    if (openEnd === -1) break;
    let body = src.slice(start + 9, openEnd);
    const selfClosing = body.trimEnd().endsWith("/");
    if (selfClosing) body = body.trimEnd().slice(0, -1);
    // pick quote style from the Size attribute itself; require icon size + no aria-label
    let q = null;
    if (/Size="ButtonSize\.Icon(?:Sm|Lg|Xl|Xs)?"/.test(body)) q = '"';
    else if (/Size=""ButtonSize\.Icon(?:Sm|Lg|Xl|Xs)?""/.test(body)) q = '""';
    if (!q || /aria-label/i.test(body)) { out += src.slice(pos, openEnd + 1); pos = openEnd + 1; continue; }
    const close = src.indexOf("</ButtonCn>", openEnd);
    const inner = selfClosing ? "" : (close === -1 ? "" : src.slice(openEnd + 1, close));
    if (/sr-only/.test(inner)) { out += src.slice(pos, openEnd + 1); pos = openEnd + 1; continue; }
    const genericMatch = inner.match(/<LucideIconCn[^>]*Name="+([a-z0-9-]+)"+/);
    const concreteMatch = inner.match(/<Lucide(?!IconCn)([A-Za-z0-9]+)Cn\b/);
    let icon = genericMatch ? genericMatch[1] : (concreteMatch ? concreteMatch[1] : null);
    if (icon && genericMatch) icon = icon.split("-").map(w => w[0].toUpperCase() + w.slice(1)).join("");
    let label = icon ? labelFor(icon) : "Action";
    // Context: in chat/AI blocks an up-arrow icon button is the send action
    if ((icon === "ArrowUp" || icon === "ArrowDown") && /[/\\](Chat|Ai)[/\\]/.test(fp)) label = "Send message";
    const tag = "<ButtonCn" + body.replace(/\s+$/, "") + ` aria-label=${q}${label}${q}` + (selfClosing ? " /" : "") + ">";
    out += src.slice(pos, start) + tag;
    pos = openEnd + 1;
    changed++;
    stats.push(label);
  }
  out += src.slice(pos);
  if (changed && WRITE) await writeFile(fp, out);
  return { changed, stats };
}

async function main() {
  const files = [];
  for await (const f of glob(TARGET.replaceAll("\\", "/") + "/**/*.razor")) files.push(f);
  let total = 0, fileCount = 0;
  const labelCounts = {};
  for (const f of files) {
    const { changed, stats } = await processFile(f);
    if (changed) { fileCount++; total += changed; for (const s of stats) labelCounts[s] = (labelCounts[s] || 0) + 1; }
  }
  console.log(`${WRITE ? "APPLIED" : "DRY-RUN"}: ${total} aria-labels across ${fileCount} files`);
  const top = Object.entries(labelCounts).sort((a, b) => b[1] - a[1]).slice(0, 25);
  for (const [l, n] of top) console.log(`  ${String(n).padStart(4)}  ${l}`);
}
main().catch(e => { console.error(e); process.exit(1); });
