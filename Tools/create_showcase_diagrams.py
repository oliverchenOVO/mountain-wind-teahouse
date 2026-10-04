"""Generate editable SVG portfolio diagrams from verified project structure."""
from html import escape
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'ShowcaseDraft/images'
INK, GREEN, PAPER, GOLD = '#213f37', '#416b58', '#f6f3e8', '#ae8136'


def start(title, description, height):
    return [f'''<svg xmlns="http://www.w3.org/2000/svg" width="1200" height="{height}" viewBox="0 0 1200 {height}" role="img" aria-labelledby="title desc">
<title id="title">{escape(title)}</title><desc id="desc">{escape(description)}</desc>
<defs><marker id="arrow" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="8" markerHeight="8" orient="auto-start-reverse"><path d="M0 0 L10 5 L0 10 Z" fill="{GREEN}"/></marker></defs>
<style>text{{font-family:'Microsoft JhengHei','Noto Sans TC',sans-serif;fill:{INK}}}.head{{font-size:32px;font-weight:700}}.body{{font-size:23px}}.small{{font-size:20px}}.title{{font-size:42px;font-weight:700}}.arrow{{fill:none;stroke:{GREEN};stroke-width:3;marker-end:url(#arrow)}}.optional{{stroke-dasharray:7 6}}</style>
<rect width="1200" height="{height}" rx="22" fill="{PAPER}"/>
<rect x="40" y="40" width="7" height="66" rx="3" fill="{GOLD}"/>
<text class="title" x="68" y="78">{escape(title)}</text>''']


def text(parts, x, y, value, cls='body', anchor='start'):
    parts.append(f'<text class="{cls}" x="{x}" y="{y}" text-anchor="{anchor}">{escape(value)}</text>')


def node(parts, x, y, w, h, title, lines, fill='#e3eadf', detail='body'):
    parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="20" fill="{fill}" stroke="#bdcbbd"/>')
    text(parts, x+24, y+39, title, 'head')
    for i, line in enumerate(lines):
        text(parts, x+24, y+75+i*30, line, detail)


def arrow(parts, path, optional=False):
    parts.append(f'<path class="arrow{" optional" if optional else ""}" d="{path}"/>')


def save(parts, name):
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / name).write_text('\n'.join(parts) + '\n</svg>\n', encoding='utf-8')


def daily():
    p = start('山中的一天 · 每日玩法流程', '採集與備餐、主動開店、送餐與互動、休息跨日；故事及旅行都是可選活動。', 1110)
    text(p, 68, 118, '0.33｜建議節奏，不是強制任務順序', 'small')
    node(p, 190, 155, 440, 115, '01  白天探索與採集', ['茶葉、香菇、竹筍與溪魚', '查看天氣、菜單與備餐目標'])
    node(p, 190, 310, 440, 115, '02  回茶屋煮茶、備餐', ['補足今晚菜單上的料理', '金色火候區按 Space 完成'])
    node(p, 190, 465, 440, 115, '03  主動開店', ['回溪谷茶屋，在料理台開店', '上架料理至少 3 份才能營業'], '#efe3c9')
    node(p, 190, 620, 440, 115, '04  夜間桌位送餐', ['看訂單 → 拿托盤 → 走到桌邊', '按 E 交付餐點'])
    node(p, 190, 775, 440, 115, '05  客人互動與結帳', ['客人用餐後聊天、結帳', '查看收入與經營成果'])
    node(p, 190, 930, 440, 115, '06  回房休息、進入隔日', ['保留材料、料理、金錢與進度', '採集點刷新，回到溪谷茶屋'])
    for y in (270, 425, 580, 735, 890):
        arrow(p, f'M410 {y} V{y+40}')
    arrow(p, 'M190 987 H125 V210 H190')
    text(p, 82, 612, '隔日', 'small', 'middle')
    node(p, 700, 155, 450, 320, '白天可自由插入的活動', [
        '與文、荷取、椛聊聊近況', '採集／釣魚、委託與好感故事', '瀑布山路旅行、巡山便當',
        '符卡練習、亭旁長凳歇腳', '材料雜貨、菜單與庭院升級', '七日手帖、小祭典與天氣事件'], '#ecebdf')
    arrow(p, 'M630 211 H700', True)
    arrow(p, 'M700 400 H667 V365 H630', True)
    node(p, 700, 530, 450, 175, '傍晚不會自動開店', ['可以繼續備餐或自由探索', '想營業時再主動選擇開店', '也可跳過營業，直接回房休息'], '#efe3c9')
    node(p, 700, 755, 450, 205, '持續遊玩的目標', ['修繕招牌是 Demo 主要目標', '完成後仍可繼續山中生活', '地圖只提示方向，不自動尋路', '實線：建議循環／虛線：可選活動'], '#ecebdf')
    text(p, 68, 1080, '採集、交易與跨日等會保存進度；F5 手動存檔。開始新旅程會覆寫目前進度。', 'small')
    save(p, 'daily-flow.svg')


def architecture():
    p = start('山風茶屋 · 系統架構', '單機 Unity 原型，以一個 MountainTeaGame partial class 整理遊戲功能，共用 SaveData，JSON 本機存檔及程序生成呈現。', 1120)
    text(p, 68, 118, '0.33｜依現有程式整理；框內是功能分組，不是獨立服務或資料庫', 'small')
    node(p, 300, 151, 600, 112, 'Unity 執行期與玩家操作', ['Boot / Awake → Update / LateUpdate / OnGUI'])
    arrow(p, 'M600 263 V301')
    p.append('<rect x="40" y="301" width="1120" height="505" rx="24" fill="#edf0e5" stroke="#91aa98" stroke-width="2"/>')
    text(p, 66, 341, 'MountainTeaGame : MonoBehaviour', 'head')
    text(p, 66, 373, '多個 partial 檔案，編譯後仍是同一個類別；共用欄位與狀態', 'body')
    node(p, 65, 400, 340, 130, '探索與旅行', ['MountainTrip / LivingMountain', 'TravelMap / TrailRestActions'], detail='small')
    node(p, 430, 400, 340, 130, '料理與營業', ['CafeService / CookingLife', 'TeaPlanning / TeaLedger'], detail='small')
    node(p, 795, 400, 340, 130, '人物與生活事件', ['MountainStories / FamiliarChats', 'TeaWeek / MountainFestival'], detail='small')
    node(p, 65, 550, 340, 115, '日夜、天氣與經營', ['RainWeather / TeaUpgrades', 'DailySpecial / MountainSupplies'], detail='small')
    node(p, 430, 550, 340, 115, '介面與操作引導', ['TeaUITheme / ContextGuide', 'ServiceReadability / OnGUI'], detail='small')
    node(p, 795, 550, 340, 115, '視線與音訊設定', ['CameraCanopies / TeaRoofView', 'PavilionRoofView / AudioSettings'], detail='small')
    node(p, 65, 697, 1070, 82, '共用 SaveData：日期、庫存、任務、客人、好感、經營與旅行進度', [], '#e0e7d7')
    arrow(p, 'M235 665 V697')
    arrow(p, 'M600 665 V697')
    arrow(p, 'M965 665 V697')
    node(p, 40, 865, 540, 165, '本機持久化', ['Save / Load + JsonUtility', 'JSON 遊玩進度、備份與資料相容處理', '音量設定另存 audio-settings.json'])
    node(p, 620, 865, 540, 165, '場景與角色呈現', ['MountainArt / TrailArt / TeaHouseWorld', 'Resources 角色 FBX + AvatarMotion', '材質、粒子、角色表情與程序合成音訊'])
    arrow(p, 'M310 779 V865')
    text(p, 330, 842, '保存／載入', 'small')
    arrow(p, 'M1135 600 H1180 V836 H890 V865')
    text(p, 900, 826, '建立／更新呈現', 'small')
    text(p, 68, 1076, '開發流程：Blender Python → blend / FBX → Unity Resources；BuildDemo → Windows 建置。', 'small')
    text(p, 68, 1104, 'QA 斷言與截圖是本機開發驗證；目前沒有伺服器、雲端帳號或資料庫後端。', 'small')
    save(p, 'system-architecture.svg')


if __name__ == '__main__':
    daily()
    architecture()
