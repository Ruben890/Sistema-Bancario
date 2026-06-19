import type { LucideIcon } from "lucide-react";

interface StatCardProps {
  label: string;
  value: string | number;
  icon: LucideIcon;
}

export const StatCard = ({ label, value, icon: Icon }: StatCardProps) => (
  <article className="stat-card">
    <div>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
    <Icon size={22} />
  </article>
);
